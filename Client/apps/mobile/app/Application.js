/**
 * The main application class. An instance of this class is created by app.js when it
 * calls Ext.application(). This is the ideal place to handle application launch and
 * initialization details.
 */
Ext.define('GPNBot.Application', {
    extend: 'Ext.app.Application',

    requires: [
        'GPNBot.helpers.Settings',
        'GPNBot.helpers.WebSocketConnection',
        'SU.native.Network',
        'SU.storage.LocalStorageCookie'
    ],

    controllers: [
        'Routes'
    ],

    name: 'GPNBot',
    quickTips: false,

    defaultToken: 'main',

    init: function () {
        var me = this;

        me.callParent(arguments);

        Ext.override(Ext.field.Field, {
            config: { errorTarget: 'under' /*'side'*/ }
        });

        me.initAjax();

        //Ext.mixin.Responsive.activate();

    },

    launch: function () {
        var me = this;
        
        // Скрываем Splashscreen кордовы
        navigator && navigator.splashscreen && navigator.splashscreen.hide();

        // Скрываем наш Splashscreen
        //setTimeout(function () {
        //    me.destroyLoader();
        //}, 1000 * 1);

        // Не сигналим состояние сети
        //SU.native.Network.setSignalling(true);

        //window.StatusBar && StatusBar.backgroundColorByHexString("#e72a5f");
        window.Keyboard && Keyboard.disableScrollingInShrinkView && Keyboard.disableScrollingInShrinkView(true);
    },

    onAppUpdate: function () {
        window.location.reload();
    },

    privates: {

        initAjax: function () {
            var me = this;

            // Установка токена авторизации для каждого ajax-запроса
            Ext.Ajax.on({
                beforerequest: function (conn, options) {
                    var token = GPNBot.Settings.getToken();

                    if (!token) return;

                    if (!options.headers) {
                        options.headers = {};
                    }

                    options.headers['Authorization'] = 'Bearer ' + token;

                    if (options.header) {
                        options.header['Authorization'] = 'Bearer ' + token;
                    }

                },

                // Перехват Ajax-ошибок
                requestexception: function (ajax, resp) {
                    me.checkAjaxResponse(resp);
                    //console.log('ajax exception', arguments);
                },

                requestcomplete: function (ajax, resp) {
                    me.checkAjaxResponse(resp);
                    //console.log('ajax complete', arguments);
                }

            });

            //// Перехват Ajax-ошибок
            //Ext.override(Ext.data.proxy.Server, {
            //    constructor: function (config) {
            //        this.callOverridden([config]);
            //        this.addListener({
            //            exception: function (ajax, resp) {
            //                //me.checkAjaxOnErrors(resp);
            //                //console.log('Ext.data.proxy.Server exception', arguments);
            //            }
            //        });
            //    }
            //});


        },

        errorShow: function (options) {
            var me = this;

            options = options || {};
            options.message = options.message || '';
            delete options.requestId;
            delete options.description;

            console.warn(options);
            //Ext.toast(options.message);
            //me.pushsheet.addMessage({
            //    body: options.message,
            //    title: 'Извините, что-то пошло не так'
            //});

            //<debug>
            //Ext.Msg.alert('Извините, что-то пошло не так', options.message, options.callback, options.scope );
            //return;
            //</debug>

            Ext.callback(options.callback, options.scope || this);
        },

        // Проверка Ajax-запроса на ошибки
        checkAjaxResponse: function (response) {

            var statusCode = parseInt(response.status),
                statusText = response.statusText,
                requestUrl = response.request.url,
                method = response.request.method,

                errorControl = !(response.request.headers
                    && response.request.headers['No-Response-Control-Error']
                    && response.request.headers['No-Response-Control-Error'] === 'true'),

                mjson = {};

            if (response.aborted) return false;

            if (response.responseText) {
                // разбор полета по ответу сервера
                try {
                    mjson = JSON.parse(response.responseText);
                } catch (e) {
                    mjson = {
                        success: false,
                        message: response.responseText
                    };
                }

                // например 401 или 0
                // if (statusCode !== 200) {
                //     errorMsg = statusText + ' (' + statusCode + ')';
                // }

                // Обработка стандартного M_JSON
                //if (mjson.success !== undefined && mjson.message !== undefined && mjson.success !== true) {
                //    errorMsg = mjson.message;
                //}

            } else {

                //console.log(response.request.timedout, response.request);

                if (response.request.timedout) {
                    mjson = {
                        success: false,
                        message: 'Похоже, что сервер долго реагирует.\nЭто может быть вызвано либо плохой связью, либо ошибкой работы наших серверов.\nПожалуйста, попробуйте еще раз через некоторое время'
                    };
                } else if (response.status === 0 && !Ext.isOnline()) {
                    mjson = {
                        success: false,
                        message: 'Похоже, у вас сейчас нестабильная сеть.\nПожалуйста, попробуйте еще раз, когда сеть стабилизируется'
                    };
                } else if (statusCode !== 0) {
                    mjson = {
                        success: false,
                        message: statusText + ' (' + statusCode + ')'
                    };
                } else {
                    mjson = {
                        success: false,
                        message: 'Ошибка сети ' // + (response.request.url ? 'в запросе ' + response.request.url : statusText + ' (' + statusCode + ')')
                    };
                }

            }

            if (statusCode === 403) { //'Не авторизовано'
                this.errorShow({
                    message: 'Ваш сеанс недействителен или истек его срок',
                    callback: this.logout,
                    scope: this
                });
                return true;
            }

            if (statusCode === 401) { //'Зашли на другом устройстве'
                Ext.toast({
                    message: 'В Ваш аккаунт выполнен вход с другого устройства',
                    timeout: 3000,
                    position: 'center',
                    addPixelsY: -150,
                    styling: {
                        opacity: 1,
                        backgroundColor: '#F1F1F1',
                        textColor: '#000000',
                        cornerRadius: 40,
                        horizontalPadding: 40,
                        verticalPadding: 20
                    }
                });
                Ext.defer(Ext.bind(this.logout, this, []), 3000);
                return true;
            }

            description = Ext.String.format(
                'Request\n ({0}) {1}\n\nResponse\nCode: ({2}) {3}\n{4}',
                method,
                requestUrl,
                statusCode,
                statusText,
                response.responseText
            );

            if (mjson.success !== true && errorControl) {

                if (!~[200, 401, 403].indexOf(statusCode)) {
                    mjson.message = (mjson.message || '') + '\n' + statusText + ' (' + statusCode + ')\n';
                }

                this.errorShow(mjson);
                return false;
            }

            return true;
        }
    },

    logout: function () {
        this.redirectTo('auth/logout');
    }

});
