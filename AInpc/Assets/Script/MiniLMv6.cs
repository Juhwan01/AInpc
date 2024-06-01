using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;
using System.IO;
using System.Text;
using FF = Unity.Sentis.Functional;

/*
 *              Tiny Stories Inference Code
 *              ===========================
 *  
 *  Put this script on the Main Camera
 *  
 *  In Assets/StreamingAssets put:
 *  
 *  MiniLMv6.sentis
 *  vocab.txt
 * 
 *  Install package com.unity.sentis
 * 
 */


public class MiniLM : MonoBehaviour
{
    private static MiniLM instance;
    const BackendType backend = BackendType.GPUCompute;

    //Special tokens
    const int START_TOKEN = 101; 
    const int END_TOKEN = 102; 

    //Store the vocabulary
    string[] tokens;

    const int FEATURES = 384; //size of feature space

    IWorker engine, dotScore;

    public static MiniLM Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("ServerController2 instance is null. Make sure the script is attached to an active GameObject in the scene.");
            }
            return instance;
        }
    }

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    public void getScore(string str1,string str2) 
    {
        tokens = File.ReadAllLines(Application.streamingAssetsPath + "/vocab.txt");

        engine = CreateMLModel();

        dotScore = CreateDotScoreModel();

        var tokens1 = GetTokens(str1);
        var tokens2 = GetTokens(str2);

        using TensorFloat embedding1 = GetEmbedding(tokens1);
        using TensorFloat embedding2 = GetEmbedding(tokens2);

        float score = GetDotScore(embedding1, embedding2);

        Debug.Log("Similarity Score: " + score);
    }
    float GetDotScore(TensorFloat A, TensorFloat B)
    {
        var inputs = new Dictionary<string, Tensor>()
        {
            { "input_0", A },
            { "input_1", B }
        };
        dotScore.Execute(inputs);
        var output = dotScore.PeekOutput() as TensorFloat;
        output.CompleteOperationsAndDownload();
        return output[0];
    }

    TensorFloat GetEmbedding(List<int> tokens)
    {
        int N = tokens.Count;
        using var input_ids = new TensorInt(new TensorShape(1, N), tokens.ToArray());
        using var token_type_ids = new TensorInt(new TensorShape(1, N), new int[N]);
        int[] mask = new int[N];
        for (int i = 0; i < mask.Length; i++)
        {
            mask[i] = 1;
        }
        using var attention_mask = new TensorInt(new TensorShape(1, N), mask);

        var inputs = new Dictionary<string, Tensor>
        {
            {"input_0", input_ids },
            {"input_1", attention_mask },
            {"input_2", token_type_ids}
        };

        engine.Execute(inputs);

        var output = engine.TakeOutputOwnership("output_0") as TensorFloat;
        return output;
    }

    IWorker CreateMLModel()
    {
        Model model = ModelLoader.Load(Application.streamingAssetsPath + "/MiniLMv6.sentis");

        Model modelWithMeanPooling = Functional.Compile(
          (input_ids, attention_mask, token_type_ids) =>
          {
              var tokenEmbeddings = model.Forward(input_ids, attention_mask, token_type_ids)[0];
              return MeanPooling(tokenEmbeddings, attention_mask);
          },
          (model.inputs[0], model.inputs[1], model.inputs[2])
        );

        return WorkerFactory.CreateWorker(backend, modelWithMeanPooling);
    }

    //Get average of token embeddings taking into account the attention mask
    FunctionalTensor MeanPooling(FunctionalTensor tokenEmbeddings, FunctionalTensor attentionMask)
    {
        var mask = attentionMask.Unsqueeze(-1).BroadcastTo(new[] { FEATURES });     //shape=(1,N,FEATURES)
        var A = FF.ReduceSum(tokenEmbeddings * mask, 1, false);                     //shape=(1,FEATURES)       
        var B = A / (FF.ReduceSum(mask, 1, false) + 1e-9f);                         //shape=(1,FEATURES)
        var C = FF.Sqrt(FF.ReduceSum(FF.Square(B), 1, true));                       //shape=(1,FEATURES)
        return B / C;                                                               //shape=(1,FEATURES)
    }

    IWorker CreateDotScoreModel()
    {
        Model dotScoreModel = Functional.Compile(
            (input1, input2) => Functional.ReduceSum(input1 * input2, 1),
            (InputDef.Float(new TensorShape(1, FEATURES)),
            InputDef.Float(new TensorShape(1, FEATURES)))
        );

        return WorkerFactory.CreateWorker(backend, dotScoreModel);
    }

    List<int> GetTokens(string text)
    {
        //split over whitespace
        string[] words = text.ToLower().Split(null);

        var ids = new List<int>
        {
            START_TOKEN
        };

        string s = "";

        foreach (var word in words)
        {
            int start = 0;
            for(int i = word.Length; i >= 0;i--)
            {
                string subword = start == 0 ? word.Substring(start, i) : "##" + word.Substring(start, i-start);
                int index = System.Array.IndexOf(tokens, subword);
                if (index >= 0)
                {
                    ids.Add(index);
                    s += subword + " ";
                    if (i == word.Length) break;
                    start = i;
                    i = word.Length + 1;
                }
            }
        }

        ids.Add(END_TOKEN);

        Debug.Log("Tokenized sentece = " + s);

        return ids;
    }

    private void OnDestroy()
    { 
        dotScore?.Dispose();
        engine?.Dispose();
    }

}
