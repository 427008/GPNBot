/**
 * 
 */
Ext.define('GPNBot.view.main.properties.Properties', {
    extend: 'Ext.Container',
    xtype: 'properties',

    listeners: {
        listeners: {
            activate: function () {
                var main = this.up('main');
                main.changeToolbarButton(null);
                main.setTitle('Настройки')
            }
        },

        deactivate: function () {
            this.destroy();
        },

        //activeitemchange: function (me, item, oldItem) {
        //    field = me.down('searchfield');
        //    field.setValue(item.searchParam || '');
        //}
    },

});
