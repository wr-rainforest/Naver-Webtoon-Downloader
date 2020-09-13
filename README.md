# Naver-Webtoon-Downloader
![GitHub release (latest by date)](https://img.shields.io/github/v/release/wr-rainforest/Naver-Webtoon-Downloader?label=latest&style=flat-square)
![GitHub All Releases](https://img.shields.io/github/downloads/wr-rainforest/Naver-Webtoon-Downloader/total?label=Downloades&style=flat-square)   
네이버 웹툰 다운로더입니다.   
.Net Frmamework 4.7.2 이상 버전에서 동작합니다.    
윈도우10 2018년 4월 업데이트(버전 1803) 이후 버전을 사용중이시라면 런타임 추가 설치 없이도 실행이 가능합니다.   
[.Net Frmamework 4.7.2 다운로드](https://dotnet.microsoft.com/download/dotnet-framework/net472)


# Version 
- __v1.0.*.*(개발중)__
  - 버전 명명규칙이 변경되었습니다.
  - 커맨드에서 titleId 여러개를 한번에 입력이 가능해집니다. 다운로드는 순차적으로 이루어집니다.
  - 커맨드 문자열 관련 버그들을 수정할 계획입니다.
  - 네이버 웹툰 페이지의 HTML DOM이 변경되어 제대로 동작하지 않을 경우를 대비하여, 서버에서 노드의 xpath 텍스트를 받아오는 기능을 추가할 계획입니다.



- __v0.3.0.34414__
  - 특이사항
    - 마지막 pre-Releases입니다.
    - 버전 명명규칙이 변경되었습니다.
    - 이번 버전은 하드코딩된 문자열이 많습니다. 추후 제대로 동작하지 않을 가능성이 상당히 높습니다.
  - 업데이트 내용
    - 인터페이스가 대거 수정되었습니다. 커맨드 기반으로 동작합니다.
    - 메인페이지에서 최신 버전 확인이 가능해졌습니다.
    - 웹툰의 titleId 목록을 불러오는 get 커맨드가 추가되었습니다.
  - 버그
    - 스페이스를 두개 이상 붙여서 입력할 시 커맨드 옵션을 인식하지 못합니다



- __v0.2.0-alpha__
  - 업데이트 내용
    - 인터페이스가 추가되었습니다.
    - 입력된 웹툰 titleId가 실제로 서버상에 존재하는지 확인이 가능해졌습니다.
    - 현재 메타데이터 캐시 생성 진행상황을 확인하는 기능이 추가되었습니다.
    - 현재 설정사항을 메인 콘솔창에 보여주는 기능이 추가되었습니다.   



- __v0.1.0-alpha__
  - 업데이트 내용
    - 다운로드 기능만 구현된 초기 버전입니다.   
    - 명령줄 인수로 titleId를 입력받아 동작합니다.
    - 현재 다운로드 진행상황을 볼 수 있습니다.
  - 버그
    - titleId가 존재하지 않을시 NullReferenceException을 throw 합니다.