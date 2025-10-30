## Описание
Проект представляет собой административное приложение для управления пользователями, меню и бронированиями, с возможностью авторизации и редактирования данных.

<img src="images/1-Screenshot_3.jpg" width="300">
<img src="images/2-Screenshot_3.jpg" width="500">
<img src="images/3-Screenshot_3.jpg" width="500">
<img src="images/4-Screenshot_3.jpg" width="500">
<img src="images/5-Screenshot_3.jpg" width="500">
<img src="images/6-Screenshot_3.jpg" width="500">
<img src="images/7-Screenshot_3.jpg" width="500">
<img src="images/8-Screenshot_3.jpg" width="500">
<img src="images/9-Screenshot_3.jpg" width="500">

## Возможности

- Авторизация пользователей через форму входа.

- Панель администратора с доступом к данным пользователей, меню и бронирований.

- Добавление, редактирование и удаление записей (CRUD-функционал).

- Работа с JSON и асинхронными операциями через встроенные библиотеки .NET.

- Локализация интерфейса с использованием `.resx` файлов ресурсов.

## Используемые технологии

- **C# / .NET Framework 4.7.2**
- **Windows Forms**
- **NuGet Packages:**
  - `System.Text.Json`
  - `System.Memory`
  - `System.IO.Pipelines`
  - `Microsoft.Bcl.AsyncInterfaces`
  - `System.ValueTuple`
- **IDE:** Visual Studio

## Запуск и установка

1. Склонируйте репозиторий:

   ```bash
   git clone https://github.com/Tat-T/WinFormsRest.git

2. Откройте решение в Visual Studio (WindowsAdminApp.sln).

3. Восстановите NuGet-пакеты:

   ```bash
   Tools → NuGet Package Manager → Restore Packages

4. Выберите конфигурацию Debug или Release.

5. Запустите проект (F5) или соберите исполняемый файл.