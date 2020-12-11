/**
 * 
 */
Ext.define('GPNBot.helpers.Settings', {
    alternateClassName: ['GPNBot.HelperSettings', 'GPNBot.Settings'],
    singleton: true,

    requires: [
        'SU.storage.LocalStorageCookie',
        'Ext.util.History',
        'GPNBot.helpers.CurrentUser',
        'GPNBot.store.Service'
    ],

    config: {

        /**
         * @cfg {String} token
         * Текущий токен авторизации пользователя
         * @private
         */

        /**
         * @cfg {String} expires
         * Время жизни текущего токена авторизации пользователя
         * @private
        */

        /**
         * @cfg {String} startRoute
         * Начальный адрес перехода при запуске
         */
        startRoute: undefined

    },

    _token: undefined,

    _expires: undefined,


    constructor: function (config) {
        var me = this,
            login;

        Ext.Cookie.setProxyId('gpnbot');

        // Получаем токен авторизации только после инициализации ProxyId
        me._token = Ext.Cookie.get('access_token');
        me._expires = Ext.Cookie.get('expires_token');

        me.callParent(arguments);
        me.initConfig(config);

        me.setStartRoute(Ext.util.History.currentToken);
    },

    getToken: function () {
        return this._token;
    },

    setToken: function (newToken, expires, remember) {
        if (remember) {
            Ext.Cookie.set('access_token', newToken);
            Ext.Cookie.set('expires_token', expires);
        } else {
            Ext.Cookie.del('access_token');
            Ext.Cookie.del('expires_token');
        }

        this._token = newToken;
    },

    getExpires: function () {
        return this._expires;
    },


    /**
     * Загрузка данных пользователя по запросу GetUserInfo,
     * получение профиля пользователя,
     * получение профиля компании, если надо,
     * прочие загрузки необходимые для старта
     * 
     * @return {Ext.Promise} Результат загрузки
     */
    loadSettings: function () {
        var me = this;

        return me.getUserInfo()
            .then(Ext.bind(me.getServices, me));
    },

    getUserInfo: function () {
        var me = this;

        // посылаем настоящий запрос
        return Ext.Ajax.request({
            url: (Ext.APIUrl || '') + '/userinfo'
        })
            .then(

                // Успешно
                function (response) {
                    var json;

                    try {
                        json = JSON.parse(response.responseText);
                    } catch (e) {
                        json = {
                            success: false,
                            message: e.toString()
                        };
                    }

                    return json;

                },

                // Ошибка
                function (response) {

                    if (response.status !== 0) {
                        return {
                            success: false,
                            message: response.responseText || response.statusText
                        };
                    }

                    return {
                        success: false,
                        message: Ext.isOnline() ? 'Ошибка сервера' : 'Нет интернет соединения'
                    };

                }

            )
            .then(
                function (json) {

                    if (json.success === false) {
                        throw Ext.raise(json.message);
                    }

                    GPNBot.CurrentUser.set(json.data);
                    GPNBot.CurrentUser.commit();

                }
            );
    },

    getServices: function () {
        var service = Ext.getStore('service');

        return service.load();
    }

});
