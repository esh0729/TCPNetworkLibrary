# 프로젝트 정보  
- C# .Net Core를 사용한 비동기 소켓 서버
- C# .Net Framework를 사용한 Non-Blocking 소켓 클라이언트
- APM(비동기 프로그래밍 모델), EAP(이벤트 기반 비동기 패턴), TAP(작업 기반의 비동기 패턴)의 비동기 프로그래밍 패턴을 사용한 TCP 네트워크 통신 구현
- Unity에서 사용가능하도록 클라이언트 라이브러리의 경우 .Net Framework 사용
  
# 서버
- APM(비동기 프로그래밍 모델) : IAsyncResult 인터페이스를 사용한 모델로 Begin~, End~ 메소드를 사용하여 구현
- EAP(이벤트 기반 비동기 패턴) : 비동기 소켓작업을 처리하는 SocketAsyncEventArgs 객체를 이용한 모델로 SocketAsyncEventArgs를 매개변수로 사용하는 ~Async 메소드 사용하여 구현
- TAP(작업 기반의 비동기 패턴) : Task 형식을 기반으로 작성한 모델로 Task를 반환하는 ~Async 메서드, FromAsync 메서드와 await async 키워드를 사용하여 구현
- TestServer 프로젝트를 사용하여 각 모델 테스트 구동
  
# 클라이언트
- Non-Blocking 소켓을 사용하여 단일스레드에서 구동이 가능하도록 구현
- TestClient 프로젝트를 사용하여 테스트 구동

# 내부 구조
![01](https://user-images.githubusercontent.com/100393621/201566149-fc0f5458-2be1-434c-93dd-1bee8ad8ee95.PNG)
![02](https://user-images.githubusercontent.com/100393621/201566157-eb5d6db6-e0dd-4d45-8bf8-a32a07aad2f8.PNG)
![03](https://user-images.githubusercontent.com/100393621/201566162-4e1f1283-bf25-4ffb-be9e-2b4caac53453.PNG)
![04](https://user-images.githubusercontent.com/100393621/201566167-7bc5bf75-6dd7-4305-993e-e0f925df1a9b.PNG)
![05](https://user-images.githubusercontent.com/100393621/201566170-1bcc177f-8d9f-4d37-aaab-cee11104befa.PNG)
![06](https://user-images.githubusercontent.com/100393621/201566175-b6286c90-7e41-4ad0-b3f0-005d9e31a889.PNG)
