/**
 * 
 */
Ext.define('GPNBot.view.auth.AuthController', {
    extend: 'Ext.app.ViewController',
    alias: 'controller.auth',

    authRequest: function (options) {
        var me = this;

        GPNBot.Mask.maskBody();
        Ext.Ajax.request(options)
            .then(
                function (response) {
                    var json;
                    if (response.status !== 200) {
                        throw { success: false, message: response.statusText };
                    }

                    if (response.aborted) return false;

                    try {
                        json = JSON.parse(response.responseText);
                    } catch (e) {
                        json = {
                            success: false,
                            message: e.toString() + ' \n' + response.responseText
                        };
                    }

                    return json;
                },
                function (response) {
                    var json = {};

                    if (response.responseText) {
                        // разбор полета по ответу сервера
                        try {
                            json = JSON.parse(response.responseText);
                            json.success = json.success || false;
                            if (!Ext.isString(json.message)) {
                                json.message = 'Ошибка сервера';
                            }
                        } catch (e) {
                            json = {
                                success: false,
                                message: response.responseText
                            };
                        }

                    } else {
                        if (response.request.timedout) {
                            json = {
                                success: false,
                                message: 'Похоже, что сервер долго реагирует.\nЭто может быть вызвано либо плохой связью, либо ошибкой работы наших серверов.\nПожалуйста, попробуйте еще раз через некоторое время'
                            };
                        } else if (response.status === 0 && !Ext.isOnline()) {
                            json = {
                                success: false,
                                message: 'Похоже, у вас сейчас нестабильная сеть.\nПожалуйста, попробуйте еще раз, когда сеть стабилизируется'
                            };
                        } else if (response.status !== 0) {
                            json = {
                                success: false,
                                message: response.statusText + ' (' + response.status + ')'
                            };
                        } else {
                            json = {
                                success: false,
                                message: 'Ошибка сети'
                            };
                        }
                    }

                    return json;
                }
            )
            .then(
                function (json) {
                    var form = me.lookup('authlogin'),
                        data = form.getValues();

                    GPNBot.Mask.unmaskBody();

                    if (!json) return;
                    
                    if (json.success === false) {
                        Ext.Msg.alert('Извините, что-то пошло не так', json.message);
                        return;
                    }

                    GPNBot.Settings.setToken(json.data.token, json.data.expires, data.remember);
                    me.redirectTo(GPNBot.Settings.getStartRoute() || Ext.getApplication().getDefaultToken(), { replace: true });
                }
            );
    },

    onLoginEnter: function (event, view) {
        event.stopEvent();
        console.log('onLoginEnter', arguments);
        this.onLogin();
    },

    onLogin: function () {
        var me = this,
            form = me.lookup('authlogin'),
            data = form.getValues();

        if (!form.validate()) {
            Ext.Msg.alert('Не все поля заполнены', 'Пожалуйста, заполните все обязательные поля.');
            return;
        }

        window.Keyboard && Keyboard.hide && Keyboard.hide();

        me.authRequest({
            url: (Ext.APIUrl || '') + '/token',
            method: 'POST',
            jsonData: data
        });
    }

});
