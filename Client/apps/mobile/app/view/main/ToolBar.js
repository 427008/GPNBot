/**
 * 
 */
Ext.define('GPNBot.view.main.Toolbar', {
    extend: 'Ext.Toolbar',
    xtype: 'maintoolbar',

    requires: [
        'Ext.Button'
    ],

    userCls: 'x-main-toolbar',
    ui: 'header',
    border: true,

    items: [
        {
            ui: 'header',
            iconCls: 'x-mi mi-menu',
            ripple: false,
            listeners: {
                tap: 'onToggleNavigationSize',
                buffer: 50 
            }
        },
        {
            xtype: 'title',
            flex: 1,
            style: 'overflow: hidden;'
        },
        {
            ui: 'header',
            iconCls: 'x-mi mi-forum-outline',
            align: 'right',
            ripple: false,
            action: 'notification',
            handler: function () {
                Ext.getApplication().redirectTo('main/notification');
            }
        },
        {
            ui: 'header',
            iconCls: 'x-mi mi-keyboard-backspace', // arrow-left
            align: 'right',
            action: 'back',
            ripple: false,
            hidden: true,
            listeners: {
                tap: function () {
                    Ext.util.History.back();
                },
                buffer: 50
            }
        },
        {
            ui: 'header',
            text: 'СОХРАНИТЬ',
            align: 'right',
            action: 'save',
            ripple: false,
            hidden: true,
            handler: function () {
                var view = this.up('main').getActiveItem();

                if (view.save) {
                    view.save();
                } else {
                    Ext.util.History.back();
                }
            }
        }
    ],

    applyTitle: function (value) {
        return value;
    },

    updateTitle: function (title) {
        var me = this,
            titleEl = me.down('title');
        
        titleEl.setTitle(title);
    }

});
