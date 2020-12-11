/**
 * 
 */
Ext.define('GPNBot.view.main.itemview.ItemView', {
    extend: 'Ext.Container',
    xtype: 'itemview',

    padding: 5,
    scrollable: true,

    listeners: {
        activate: function () {
            var main = this.up('main');

            main.changeToolbarButton('back');
        },
        deactivate: function () {
            this.destroy();
        }
    },

    updateRecord: function (record) {
        var me = this;

        if (!record) return;

        me.up('main').setTitle(record.get('title'));
        me.setHtml(record.get('description'));
    }

});
