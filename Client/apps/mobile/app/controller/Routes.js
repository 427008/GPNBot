/**
 * 
 */
Ext.define('GPNBot.controller.Routes', {
    extend: 'Ext.app.Controller',
    xtype: 'routes',

    requires: [
        'Ext.viewport.Viewport',
        'Ext.MessageBox',
        'GPNBot.helpers.WebSocketConnection',
        'GPNBot.view.auth.Auth',
        'GPNBot.view.main.Main',
        'GPNBot.helpers.Mask'

    ],

    listen: {
        controller: {
            '#': {
                unmatchedroute: 'onUnmatchedRoute'
            }
        }
    },

    routes: {
        'auth(/:id)?': {
            action: 'onAuth'
        },

        'main(/:id)?(/:uid)?': {
            before: 'loggedIn',
            action: 'onMain'
        },

        'back': {
            action: 'onBack'
        }
    },

    config: {
        startHash: undefined
    },

    init: function (app) {
        var me = this,
            token = Ext.History.currentToken;

        me.callParent(arguments);

        me.redirectTo('back', { replace: true });
        me.redirectTo(token, { replace: false, force: true });

    },

    onUnmatchedRoute: function (action) {
        action = action || '';

        Ext.Msg.alert('Routes', 'Action "' + action + '" not found!');
        this.redirectTo(Ext.getApplication().getDefaultToken(), { replace: true });
    },

    loggedIn: function (/*action*/) {
        var me = this,
            // action всегда последний параметр!
            action = arguments[arguments.length - 1];
        
        // открываем форму авторизации при отсутствии токена
        if (!GPNBot.Settings.getToken()) {
            me.redirectTo('auth', { replace: true });
            return;
        }

        if (!me.isAuthenticated) {
            // авторизовались, но еще не получили глобальные данные
            // первый редирект после
            // - авторизации
            // - регистрации
            // - или запуск с сохраненным токеном 

            //// Здесь бы включить интернет
            //if (!Ext.isOnline() && window.cordova && window.cordova.plugins.settings) {
            //    //console.log('openNativeSettingsTest is active');
            //    window.cordova.plugins.settings.open("wifi", function () {
            //        //console.log('opened settings', arguments);
            //    },
            //        function () {
            //            //console.log('failed to open settings', arguments);
            //        }
            //    );
            //    Ext.on({
            //        onlinechange: function (state) {
            //            loadUserInfo();
            //        },
            //        single: true
            //    });
            //    return;
            //}

            GPNBot.Mask.maskBody('Пожалуйста подождите', 'Соединение с сервером...');

            GPNBot.Settings.loadSettings()
                .then(function () {
                    me.isAuthenticated = true;
                    GPNBot.Mask.unmaskBody();
                })
                .then(
                    function () {
                        var ws = Ext.WSConnection;
                        ws.connect({
                            url: Ext.APIWebSocket + '/ws'
                            ,protocol: [GPNBot.Settings.getToken()]
                        });
                        ws.on({
                            //afterconnect: {
                            //    fn: function () {
                            //        console.log('connected');
                            //        ws.send(JSON.stringify({
                            //            dst: 'auth',
                            //            token: GPNBot.Settings.getToken()
                            //        }));
                            //    },
                            //    single: true
                            //},
                            message: function () {
                                console.log(arguments);
                            }
                        });

                        action.resume();
                    },
                    function (error) {
                        console.log(arguments);
                        //Ext.Msg.alert('Извините, что-то пошло не так', error);

                        // Сломались - идем на логин
                        GPNBot.Mask.unmaskBody();
                        me.redirectTo('auth/login', { replace: true });
                    }
            );

        } else {
            action.resume();
        }
    },

    onBack: function (id) {
        var me = this;

        console.log('back');

        if (!me.exitTimerId) {

            Ext.toast({
                message: 'Нажмите еще раз для выхода',
                timeout: 1500,
                position: 'bottom',
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

            me.exitTimerId = setTimeout(function () {
                clearTimeout(me.exitTimerId);
                me.exitTimerId = undefined;
            }, 1500);

            me.redirectTo(1);

        } else {
            me.onExit();
            //me.redirectTo(Ext.getApplication().getDefaultToken(), { replace: true });
        }
    },


    onAuth: function (id) {
        var me = this,
            fn;

        if (!id) {
            me.redirectTo('auth/login', { replace: true });
            return;
        }

        try {
        id = id.slice(1);

        if (!~['login', 'register', 'logout', 'exit', 'privacy'].indexOf(id)) {
            me.onUnmatchedRoute('auth/' + id);
            return;
        }

        if (~['logout', 'exit'].indexOf(id)) {
            fn = 'on' + Ext.String.capitalize(id);
            me[fn].call(me);
            return;
        }

        Ext.Viewport.setActiveItem('auth');
        //try {
            Ext.Viewport.getActiveItem().setActiveItem('auth' + id);
        } catch (e) {
            console.error(e);
        }
    },

    onLogout: function () {
        var main = Ext.Viewport.down('main');

        if (main) {
            main.destroy();
        }

        this.isAuthenticated = false;

        GPNBot.Settings.setToken();
        GPNBot.Settings.setStartRoute();
        GPNBot.CurrentUser.clean();
        Ext.WSConnection.disconnect();

        this.redirectTo('auth/login', { force: true, replace: true });
    },

    onExit: function () {
        if (navigator.app) {
            navigator.app.exitApp();
        } else if (navigator.device) {
            navigator.device.exitApp();
        } else {
            window.close();
        }
    },

    onMain: function (id) {
        var view,
            args = [];

        if (!id) {
            this.redirectTo('main/home', { replace: true });
            return;
        }


        Array.prototype.slice.call(arguments).forEach(function (item, index) {
            if (index % 2) args.push(item);
        });

        Ext.Viewport.setActiveItem('main');
        view = Ext.Viewport.getActiveItem();
        view.setCurrentView.apply(view, args);
    }

});
