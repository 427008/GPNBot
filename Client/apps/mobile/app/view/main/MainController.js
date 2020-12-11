/**
 * 
 */
Ext.define('GPNBot.view.main.MainController', {
    extend: 'Ext.app.ViewController',
    alias: 'controller.main',

    requires: [
    ],

    control: {
        'searchfield': {
            search: 'doServiceSearch'
        }
    },

    onToggleNavigationSize: function (button, event) {
        var nav = this.lookup('navigation');

        event.stopEvent();
        nav.onToggleNavigationSize();
    },

    doServiceSearch: function (field, value) {
        var me = this,
            initiatives = me.view.down('initiatives'),
            chat = initiatives && initiatives.down('chatcomponent');

        if (value) {
            me.redirectTo('main/initiatives');
            field.clearValue();
            chat && chat.setRevealed(false);

            Ext.defer(function () {
                initiatives = initiatives || me.view.down('initiatives');
                chat = initiatives.down('chatcomponent');
                initiatives.setGuid(null);
                chat.sendMessage({ body: value });
            }, 600);
        }
    }

});
