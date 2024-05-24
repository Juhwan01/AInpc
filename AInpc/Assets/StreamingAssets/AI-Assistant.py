#!/usr/bin/env python
# coding: utf-8

# In[1]:


import streamlit as st 
from PyPDF2 import PdfReader
from langchain.embeddings import OpenAIEmbeddings, SentenceTransformerEmbeddings
from langchain.chat_models import ChatOpenAI
from langchain.chains import ConversationalRetrievalChain, RetrievalQA
from langchain.memory import ConversationBufferWindowMemory
from langchain.vectorstores import FAISS
from langchain.document_loaders import PyPDFLoader
from langchain.text_splitter import RecursiveCharacterTextSplitter
import gdown


# In[2]:


# 필요한 라이브러리 import
from googleapiclient.discovery import build
from google.oauth2 import service_account
import os
print(os.getcwd())

# 기본 정보 입력 및 API 열기
creds_file_path = 'C:/Users/wnghk/Desktop/AInpc/AInpc/Assets/StreamingAssets/aiassistant-423712-483e458b4945.json' # 올바른 서비스 계정 JSON 파일 경로 입력
print(creds_file_path)
# 파일에서 자격 증명 불러오기
credentials = service_account.Credentials.from_service_account_file(
    creds_file_path, scopes=['https://www.googleapis.com/auth/drive.readonly', 'https://www.googleapis.com/auth/documents.readonly'])

# 서비스 객체 생성
drive_service = build('drive', 'v3', credentials=credentials)
docs_service = build('docs', 'v1', credentials=credentials)

# 폴더 ID 및 파일 이름 설정
folder_id = "1OrIj8JXzMd-VMH2QkdH8Gm55-VXyyoUh"  # 폴더 ID 입력
file_name = "스토리"  # 다운로드하려는 파일 이름 입력

# 폴더 내의 파일 검색
query = f"'{folder_id}' in parents and name = '{file_name}'"
results = drive_service.files().list(q=query, pageSize=10, fields="files(id, name)").execute()
items = results.get('files', [])

if not items:
    print('해당 이름의 파일을 찾을 수 없습니다.')
else:
    file_id = items[0]['id']
    print(f"파일 ID: {file_id}")

    # Google Docs 파일 내용 가져오기
    document = docs_service.documents().get(documentId=file_id).execute()

    # 문서 내용 추출
    content = document.get('body').get('content')

    # 텍스트 추출 함수
    def extract_text(elements):
        text = ''
        for element in elements:
            if 'paragraph' in element:
                for elem in element.get('paragraph').get('elements'):
                    if 'textRun' in elem:
                        text += elem.get('textRun').get('content')
            elif 'table' in element:
                for row in element.get('table').get('tableRows'):
                    for cell in row.get('tableCells'):
                        text += extract_text(cell.get('content'))
            elif 'tableOfContents' in element:
                text += extract_text(element.get('tableOfContents').get('content'))
        return text

    # 텍스트 내용 출력
    doc_text = extract_text(content)


# In[3]:


# PDF에서 텍스트를 가져온다
# def get_pdf_text(pdf_path):
#     google_path = 'https://drive.google.com/uc?id='
#     file_id = '1dfTqnaO0zcPdiUjP0490S6UEgFn4hqfE-Jo9We7DDmw/edit?usp=drive_link'
#     output_name = 'test.text'
#     gdown.download(google_path+file_id,output_name,quiet=False)
#     text = ""
#     pdf_reader = PdfReader(pdf_path)
#     for page in pdf_reader.pages:
#         text += page.extract_text()
#     return text

#지정된 조건에 따라 주어진 텍스트를 더 작은 덩어리로 분할
def get_text_chunks(text):
    text_splitter = RecursiveCharacterTextSplitter(
        separators="\\n",
        chunk_size=1000,
        chunk_overlap=200,
        length_function=len
    )
    chunks = text_splitter.split_text(text)
    return chunks

#주어진 텍스트 청크에 대한 임베딩을 생성하고 FAISS를 사용하여 벡터 저장소를 생성
def get_vectorstore(text_chunks):
    embeddings = SentenceTransformerEmbeddings(model_name='sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2')
    vectorstore = FAISS.from_texts(texts=text_chunks, embedding=embeddings)
    return vectorstore


# In[6]:


import os
os.environ["OPENAI_API_KEY"] = ""

#주어진 벡터 저장소로 대화 체인을 초기화
def get_conversation_chain(vectorstore):
    memory = ConversationBufferWindowMemory(memory_key='chat_history', return_message=True)  #ConversationBufferWindowMemory에 이전 대화 저장
    conversation_chain = ConversationalRetrievalChain.from_llm(
        llm=ChatOpenAI(temperature=0.7, model_name='gpt-3.5-turbo-16k-0613'),
        retriever=vectorstore.as_retriever(),
        get_chat_history=lambda h: h,
        memory=memory
    ) #ConversationalRetrievalChain을 통해 langchain 챗봇에 쿼리 전송
    return conversation_chain


# In[7]:


# 텍스트에서 청크 검색
text_chunks = get_text_chunks(doc_text)
# PDF 텍스트 저장을 위해 FAISS 벡터 저장소 만들기
vectorstore = get_vectorstore(text_chunks)
# 대화 체인 만들기
m = get_conversation_chain(vectorstore)


# In[1]:


import speech_recognition as sr
import pyttsx3

# 음성 입력 (STT)
def recognize_speech():
    recognizer = sr.Recognizer()
    with sr.Microphone() as source:
        print("질문을 해주세요.")
        audio = recognizer.listen(source)
    try:
        question = recognizer.recognize_google(audio, language="ko-KR")
        print("사용자:", question)
        return question
    except sr.UnknownValueError:
        print("음성을 이해할 수 없습니다.")
        return ""
    except sr.RequestError:
        print("음성 서비스에 접근할 수 없습니다.")
        return ""

# 음성 출력 (TTS)
def speak_response(response):
    engine = pyttsx3.init()  # 기본 드라이버로 초기화
    engine.say(response)
    engine.runAndWait()

# 질문 처리 및 답변 생성
def process_question(question):
    # 질문에 대한 답변 가져오기
    response = m({"question": question})
    if 'answer' in response:
        answer = response['answer']
    else:
        print("올바른 응답 키를 찾을 수 없습니다.")
    return answer

# if __name__ == "__main__":
#     while True:
#         question = recognize_speech()
#         if question=="멈춰":
#             break;
#         if question:
#             answer = process_question(question)
#             print(answer)
#             speak_response(answer)
if __name__ == "__main__":
    while True:
        question = recognize_speech()
        if question=="멈춰":
            break;
        if question:
            answer = process_question(question)
            print(answer)
            speak_response(answer)


# In[ ]:




