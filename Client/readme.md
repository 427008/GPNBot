# Создание workspace проекта и нативного приложения с нуля

## workspace

__Действия выполняем в папке проекта__ 

Создание workspace

```sh
$ sencha generate workspace .
```

_не забывем про точку_

Подключение framework, единого для всех приложений

```sh
$ sencha framework upgrade ext d:\Sencha\SDK\ext-6.5.2\ ext
```

## Приложения

### Создание приложения

- используем framework в папке `ext`
- namespace приложения `App`
- папка приложения `Fingerprint`

```sh
$ sencha generate app -ext -modern App ./Fingerprint
```

### Настройки приложения

__Действия выполняем в папке приложения__

```sh
$ sencha cordova init ru.skillunion.fingerprint Fingerprint
```

Правим секцию `builds` в файле `app.json`:

```json

{
    "builds": {
        "web": {
            "default": true
        },
        "android": {
            "packager": "cordova",
            "cordova": {
                "config": {
                    "platforms": "android",
                    "id": "ru.skillunion.fingerprint",
                    "name": "Fingerprint"
                }
            }
        },
        "ios": {
            "packager": "cordova",
            "cordova": {
                "config": {
                    "platforms": "ios",
                    "id": "ru.skillunion.fingerprint",
                    "name": "Fingerprint"
                }
            }
        },
        "windows": {
            "packager": "cordova",
            "cordova": {
                "config": {
                    "platforms": "windows",
                    "id": "ru.skillunion.fingerprint",
                    "name": "Fingerprint"
                }
            }
        }
    }
}
```

Для добавления пакета SU Packages  правим в файле `app.json`:

```json

{
    "locale": "ru",

    "requires": [
        "font-awesome",
        "material-icons",
        "su",
        "su-native",
        "su-locale"
    ]

}
```

- locale: включение локализации, если не используем динамическую
- requires: пакеты по необходимости _пакет `su-native` только для нативных приложений_

### Пакеты cordova для `su-native`
```
cordova plugin add cordova-plugin-dialogs
cordova plugin add cordova-plugin-x-toast
cordova plugin add cordova-plugin-pdialog
cordova plugin add cordova-plugin-network-information
cordova plugin add cordova-plugin-splashscreen
```

###В дополнение

Если не нравится названия css файлов типа `myapp-all.css`, дополняем раздел `output` файла `app.json`:

```json
{
    "output": {
        "base": "${workspace.build.dir}/${app.name}/${build.environment}",
        "css": "${app.output.css.dir}/${app.name}.css",
        "appCache": {
            "enable": false
        }
    }

}
```

или

```json
{
    "output": {
        "base": "${workspace.build.dir}/${app.name}/${build.environment}",
        "css": "${app.output.css.dir}/style.css",
        "appCache": {
            "enable": false
        }
    }

}
```
## Сборка приложений

Сборка приложения для android

```sh
$ sencha app build android
```

Сборка приложения для ios

```sh
$ sencha app build ios
```

