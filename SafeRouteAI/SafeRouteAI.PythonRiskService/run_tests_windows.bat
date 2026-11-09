@echo off
cd /d "%~dp0"
if not exist ".venv\Scripts\python.exe" (
  echo Virtual environment not found. Follow the install steps in README.md first.
  pause
  exit /b 1
)
.venv\Scripts\python.exe -m pytest -q test_service.py test_weather.py
pause
