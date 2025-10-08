# Vendy RPC Program

Modern Discord Rich Presence Manager with Material Design

## 시스템 요구사항

- **운영체제**: Windows 10 이상 (Windows 11 권장)
- **.NET**: .NET 8.0 Runtime 또는 SDK
- **Discord**: Discord 데스크톱 앱이 실행 중이어야 함

## 빌드 방법

### 1. 사전 준비

1. **Visual Studio 2022** 또는 **Visual Studio Code** 설치
2. **.NET 8.0 SDK** 설치
   - [Microsoft .NET 다운로드 페이지](https://dotnet.microsoft.com/download/dotnet/8.0)에서 다운로드
3. **Git** 설치 (소스코드 다운로드용)

### 2. 프로젝트 빌드

#### 방법 1: Visual Studio 사용 (권장)

1. 프로젝트 폴더를 Visual Studio 2022로 열기
2. `Ctrl + Shift + B` 또는 `빌드 > 솔루션 빌드` 클릭
3. 빌드 완료 후 `bin\Release\net8.0-windows\` 폴더에서 실행 파일 확인

#### 방법 2: 명령줄 사용

1. **명령 프롬프트** 또는 **PowerShell** 열기
2. 프로젝트 폴더로 이동:
   ```cmd
   cd "C:\Users\ghkim\Downloads\Vendy_RPC"
   ```
3. 프로젝트 복원 및 빌드:
   ```cmd
   dotnet restore
   dotnet build --configuration Release
   ```
4. 실행 파일 위치: `bin\Release\net8.0-windows\Vendy_RPC.exe`

### 3. 실행 방법

1. Discord 데스크톱 앱 실행
2. 빌드된 실행 파일(`Vendy_RPC.exe`) 더블클릭
3. Application ID 입력 후 연결 버튼 클릭

## 주요 기능

- Discord Rich Presence 설정
- 실시간 상태 업데이트
- 커스텀 이미지 및 버튼 설정
- 다크 모드 지원
- 시스템 트레이 최소화

## 문제 해결

### 빌드 오류 시
- .NET 8.0 SDK가 올바르게 설치되었는지 확인
- Visual Studio에서 NuGet 패키지 복원 실행

### 실행 오류 시
- Discord가 실행 중인지 확인
- 올바른 Application ID를 입력했는지 확인
- Windows Defender나 백신 프로그램이 실행을 차단하지 않는지 확인

## 라이선스

이 프로젝트는 MIT 라이선스 하에 배포됩니다.
