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
![S01](https://user-images.githubusercontent.com/100393621/201567638-a7cc2bf5-0404-4366-a82f-4903eaf94eb5.PNG)
![S02](https://user-images.githubusercontent.com/100393621/201567641-3841e935-97b9-4b80-af64-d50a5c5cc141.PNG)
![S03](https://user-images.githubusercontent.com/100393621/201567661-6cf436e3-608a-45da-b449-383bfddcef37.PNG)
![S04](https://user-images.githubusercontent.com/100393621/201567668-a0ac80b4-9b65-41bb-92da-5fd17c7e8b8d.PNG)
![S05](https://user-images.githubusercontent.com/100393621/201567672-31546865-0ecf-420c-8c45-3e872eb96328.PNG)
![S06](https://user-images.githubusercontent.com/100393621/201567675-6bd63cff-33b8-4083-844b-d10c18c67570.PNG)
