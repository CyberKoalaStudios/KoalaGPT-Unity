![YouTube Channel Views](https://img.shields.io/youtube/channel/views/UCUpVfgd42h7pwZwCTcwjp8g)
![Discord](https://img.shields.io/discord/1016305251936129094)

---

## KoalaGPT Unity Package
Официальный пакет Unity, позволяющий использовать API KoalaGPT непосредственно в игровом движке Unity.

## Как использовать

### Импорт пакета
Чтобы импортировать пакет, выполните следующие действия:
- Откройте Unity 2019 или новее
- Перейдите в `Window > Package Manager`
- Нажмите кнопку «+» и выберите  `Add package from git URL`.
- Вставьте URL-адрес репозитория `https://github.com/CyberKoalaStudios/KoalaGPT-Unity.git` и нажмите «Add».

### Настройка учетной записи CyberKoala AI
Чтобы использовать KoalaGPT API, вам необходимо иметь учетную запись CyberKoala AI. Выполните следующие действия, чтобы создать учетную запись и сгенерировать ключ API:

- Зайдите на https://beta.cyberkoala.ru/ и зарегистрируйте аккаунт
- После создания учетной записи перейдите на https://beta.cyberkoala.ru/main/dashboard
- Пополните баланс аккаунта, затем сгенерируйте новый секретный ключ и сохраните его

### Сохранение ваших учетных данных (ключа)

Чтобы делать запросы к API KoalaGPT, вам необходимо использовать персональный ключ API и название организации (если применимо). Чтобы избежать раскрытия вашего ключа API в проекте Unity, вы можете сохранить его в локальном хранилище вашего устройства.

Для этого выполните следующие действия:

- Создайте папку с именем .cyberkoala в своем домашнем каталоге (например, `C:Users\User\` для Windows или `~\` для Linux или Mac)
- Создайте файл с именем `auth.json` в папке `.cyberkoala`
- Добавьте поле api_key и поле организации (если применимо) в файл auth.json и сохраните его.
- Вот пример того, как должен выглядеть ваш файл auth.json:

```json
{
    "api_key": "ko-...er",
    "organization": "org-...MOW"
}
```

Вы также можете передать свой ключ API в KoalaGPTApi при создании его экземпляра, но это крайне **не рекомендуется!**

```csharp
var openai = new KoalaGPTApi("ko-...er");
```

**ВАЖНО:** Ваш ключ API является секретным и привязан к учетной записи.
Не делитесь ими с другими и не раскрывайте их в каком-либо клиентском коде (например, в браузерах, приложениях).
Если вы используете KoalaGPT-Unity в продуктовой среде (продакшн), обязательно запустите данный пакет на стороне сервера, где ваш ключ API может быть безопасно загружен из переменной среды или службы управления ключами.

### Отправка запросов к KoalaGPT
Вы можете использовать класс `KoalaGPTApi` для выполнения асинхронных запросов к API KoalaGPT.

Все методы являются асинхронными и доступны непосредственно из экземпляра класса `KoalaGPTApi`.

Вот пример того, как сделать запрос:

```csharp
private async void SendRequest()
{
    var _koalaGptApi = new KoalaGPTApi();
    
    var _messages = new List<Part>();
    var message = new Part();
    message.Role = "user";
    message.Content = prompt;
    
    _messages.Add(message);
        
    var request = new CreateChatCompletionRequestPrompt{
        Model="gpt4",
        Prompt="Hello!",
    };
    var response = await _koalaGptApi.CreateChatCompletionSimplePrompt(request);
}
```

### Примеры проектов
Этот пакет включает в себя примеры сцен, которые вы можете импортировать через Package Manager:

- **KoalaGPT sample:** Пример чата наподобие ChatGPT.

### Поддерживаемые версии Unity для сборок WebGL
В следующей таблице показаны поддерживаемые версии Unity для сборок WebGL:

| Версия Unity| Поддерживается|
| --- | --- |
| 2022.2.8f1 | ✅ |
| 2021.3.5f1 | ⛔ |
| 2020.3.0f1 | ✅ |

### [WIKI](https://github.com/CyberKoalaStudios/KoalaGPT-Unity/wiki)

