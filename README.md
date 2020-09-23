# Naver-Webtoon-Downloader
[![GitHub release (latest by date)](https://img.shields.io/github/v/release/wr-rainforest/Naver-Webtoon-Downloader?label=latest&style=flat-square)](https://github.com/wr-rainforest/Naver-Webtoon-Downloader/releases/latest)
[![GitHub All Releases](https://img.shields.io/github/downloads/wr-rainforest/Naver-Webtoon-Downloader/total?label=Downloades&style=flat-square)](https://github.com/wr-rainforest/Naver-Webtoon-Downloader/releases)
       
[v1.0.7571.33791 다운로드](https://github.com/wr-rainforest/Naver-Webtoon-Downloader/releases/download/v1.0.7571.33791/Naver-Webtoon-Downloader.v1.0.7571.33791.zip)
  
      
          
           
    
네이버 웹툰 다운로더입니다.   
가볍고, 신뢰성 높은 프로그램입니다.    
    
개발자 연락처 : contact@wrforest.com

.Net Frmamework 4.7.2 이상 버전에서 동작합니다.    
윈도우 2018년 4월 업데이트(버전 1803) 이후 버전에서는 런타임 추가 설치 없이도 실행이 가능합니다.     
[.Net Frmamework 4.7.2 다운로드](https://dotnet.microsoft.com/download/dotnet-framework/net472)
        



![screenshot-1.0](info/Image/v1.0-2.jpg)    
        
# 개요
- 네이버 웹툰 서버에서 로컬 다운로드 폴더로의 단방향 동기화 개념으로 작동합니다.    
    
- titleId를 입력하면 해당 웹툰의 모든 이미지를 다운로드합니다.     
    
- 새롭게 업데이트된 회차도 자동으로 찾아서 기본 다운로드 폴더에 저장합니다.     
    
- 일부 이미지를 삭제하더라도 삭제된 이미지를 찾아 자동으로 다시 다운로드합니다.   
   
- 기본 다운로드 폴더 안에 없는 파일, 프로그램의 파일 명명규칙에 어긋나는 파일들은 다운로드 받지 않은 파일로 취급합니다.    
   
- 메타데이터 캐시를 사용하여 작업 시간을 단축하고, 신뢰도 높은 파일 관리가 가능합니다.   
   
- 분할 다운로드가 가능합니다. 웹페이지 동기화 없이 메타데이터 캐시에서 중단지점을 찾아 빠르게 재시작합니다.     
   
- 일부 이미지를 삭제하였다면 메타데이터 캐시와 대조하여 삭제된 이미지를 자동으로 다시 다운로드합니다.       
   
- 업데이트된 회차를 확인하고 다운로드합니다.     
   
- 저장될 폴더명 규칙, 파일명 규칙 설정이 가능합니다. config.json 파일을 직접 수정해야합니다. 일부 특수문자 사용이 불가능합니다.
# 작업 구성 예시
[2020.07.18] 독립일기(748105) 다운로드 명령어 입력   
[2020.07.18] 독립일기(748105) 예고편(no=1) ~ 10화(no=11) 메타데이터 캐시 생성    
[2020.07.18] 독립일기(748105) 예고편(no=1) ~ 10화(no=11) 총 이미지 152장 저장    
    
[2020.07.25] 독립일기(748105) 5(no=6)화 이미지 4장 삭제     
    
[2020.08.15] 독립일기(748105) 다운로드 명령어 입력    
[2020.08.15] 독립일기(748105) 07.18 이후 업데이트된 11화(no=12) ~ 18화(no=19) 의 메타데이터를 생성해 캐시에 추가         
[2020.08.15] 독립일기(748105) 누락된 5(no=6)화 이미지 4장 + 업데이트된 이미지 116장 저장

- 총 268장 이미지 저장됨
     
# Version 
- __v1.0.7571.33791__
  - 업데이트 내용
    - 한번에 여러개의 titleId 입력이 가능해졌습니다. 다운로드는 순차적으로 이루어집니다.
    - 폴더/파일명에 사용 불가능한 반각 문자(\ / : * ? " < > |)들은 이제부터 동일한 전각 특수문자로 교체됩니다. 
    - 기본 파일명 규칙이 변경되었습니다.
    - 캐시 생성시 비어있는 EpisodeNo를 건너뜁니다. 
    - 업데이트를 자동으로 확인합니다.
    - 콘솔창 폭이 좁을 시 진행상황이 제대로 표시되지 않는 버그를 수정하였습니다.
    - 커맨드 인자 인식 버그를 수정하였습니다.
    - 다운로드 속도가 개선되었습니다.
  - 버그
    



- __v0.3.0.34414__
  - 특이사항
    - 이번 버전은 하드코딩된 문자열이 많습니다. 추후 제대로 동작하지 않을 가능성이 상당히 높습니다.
  - 업데이트 내용
    - 인터페이스가 대거 수정되었습니다. 커맨드 기반으로 동작합니다.
    - 메인페이지에서 최신 버전 확인이 가능해졌습니다.
    - 웹툰의 titleId 목록을 불러오는 get 커맨드가 추가되었습니다.
  - 버그
    - 스페이스를 두개 이상 붙여서 입력할 시 커맨드 옵션을 인식하지 못합니다.(v1.0)
    - 회차 제목의 끝이 온점이나 스페이스로 끝날 경우 " 경로의 일부를 찾을 수 없습니다." 에러가 발생합니다.(v1.0)



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
