# 프로젝트 정보  
- C# .Net Core를 사용한 비동기 소켓 서버
- C# .Net Framework를 사용한 Non-Blocking 소켓 클라이언트
- APM(비동기 프로그래밍 모델), EAP(이벤트 기반 비동기 패턴), TAP(작업 기반의 비동기 패턴)의 비동기 프로그래밍 패턴을 사용한 TCP 네트워크 통신 구현
- Unity에서 사용가능하도록 클라이언트 라이브러리의 경우 .Net Framework 사용

#. 서버
- APM : IAsyncResult 인터페이스를 사용하여 비동기 동작을 구현하는 모델로 Begin OOO, End OOO 메소드사용
- EAP : 비동기 소켓작업을 처리하는 SocketAsyncEventArgs 객체를 사용하여 비동기 동작을 구현하는 모델로, SocketAsyncEventArgs를 매개변수로 사용하는 OOO Async 메소드사용
