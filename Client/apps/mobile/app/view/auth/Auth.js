/**
 * 
 */
Ext.define('GPNBot.view.auth.Auth', {
    extend: 'Ext.tab.Panel',
    alias: 'widget.auth',

    requires: [
        'GPNBot.view.auth.AuthController',
        'GPNBot.view.auth.Login',
        //'CRPP.mobile.view.auth.Register',
        //'CRPP.mobile.view.auth.Privacy'
    ],

    viewModel: {
        data: {
            version: Ext.versions.gpn.version
        }
    },

    maxWidth: 600,
    controller: 'auth',
    layout: { type: 'card', animation: false },
    ui: 'plain',
    cls: 'x-auth',

    tabBar: {
        animateIndicator: false,
        padding: '0 40',
        ripple: false,
        defaults: {
            ui: 'plain',
            ripple: false
        }
    },

    items: [
        // логотип
        {
            xtype: 'container',
            docked: 'top',
            cls: 'x-logo-big'
        },

        // версия
        {
            xtype: 'container',
            docked: 'bottom',
            cls: 'x-version',
            bind: {
                html: '{version}'
            }
        },

        {
            xtype: 'authlogin',
            title: 'Авторизация'
        }
        //{
        //    xtype: 'auth-register',
        //    title: 'Регистрация'
        //}
    ],

    listeners: {
        add: 'onAddItem',
        deactivate: 'onDeactivate',
        activeItemchange: 'onActiveItemchanged',
        scope: 'this'
    },

    onAddItem: function (sender, item, index) {
        if (index > 1) {
            item.tab.hide();
            if (item.getTitle()) {
                item.allowHeader = true;
                item.setHeader({});
                item.setTitle(item.getTitle());
            }
        }
    },

    onDeactivate: function () {
        Ext.destroy(this);
    },

    /**
     * При переключении вкладок
     * показывает TabBar для форм 
     * {@link CRPP.mobile.view.auth.Login авторизации} и 
     * {@link CRPP.mobile.view.auth.Register регистрации}
     * и перенаправляет роутинг
     * 
     * @param {ELP.Web.view.auth.Auth} sender контейнер
     * @param {Ext.Container} newTab новая форма
     * @param {Ext.Container} oldTab старая форма
     */
    onActiveItemchanged: function (sender, newTab, oldTab) {
        var viewIndex = sender.getActiveItemIndex();

        sender.getTabBar().setHidden(viewIndex > 1);
        newTab.focus();

        Ext.getApplication().redirectTo(
            newTab.xtype.replace(/-/g, '/'),
            { replace: viewIndex > 1 }
        );
    }

});
