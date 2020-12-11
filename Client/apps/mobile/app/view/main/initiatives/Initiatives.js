/**
 * 
 */
Ext.define('GPNBot.view.main.initiatives.Initiatives', {
    extend: 'Ext.Container',
    alias: 'widget.initiatives',
    requires: [
        'GPNBot.view.main.initiatives.ChatComponent'
    ],

    config: {
        guid: undefined,
        title: undefined
    },

    layout: 'fit',

    items: [
        {
            xtype: 'chatcomponent'
        }
    ],

    listeners: {
        activate: function () {
            var main = this.up('main');

            main.changeToolbarButton('back');
            main.setTitle(this.getTitle());
            this.down('chatcomponent').store.doReload();
        },
        deactivate: function () {
            this.down('chatcomponent').store.clearTimeout();
            //this.setGuid(null);

        }
    },

    updateGuid: function (value) {
        var me = this,
            chat = me.down('chatcomponent'),
            service = Ext.getStore('service').queryRecords('guid', value)[0];

        chat.setGuid(value);
        me.setTitle(service ? service.get('title') : 'Чат бот');
        Ext.getApplication().redirectTo('main/initiatives' + (value ? '/' + value : ''), { replace: true });
    },

    updateTitle: function (value) {
        var main = this.up('main');

        if (main && value) {
            main.setTitle(value);
        }
    }

});
