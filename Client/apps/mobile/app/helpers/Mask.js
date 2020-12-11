/**
 * 
 */
Ext.define('GPNBot.helpers.Mask', {
    alternateClassName: 'GPNBot.Mask',

    singleton: true,

    requires: [
        'Ext.LoadMask'
    ],

    constructor: function () {
        this.isNative = !!(window.cordova && cordova.plugin && cordova.plugin.pDialog);

        return this.callParent(arguments);
    },

    maskBody: function (title, message) {
        var isNative = !!(window.cordova && cordova.plugin && cordova.plugin.pDialog);

        if (isNative) {
            cordova.plugin.pDialog.init({
                //theme: 'HOLO_DARK',
                //progressStyle: 'SPINNER',
                cancelable: false,
                title: title,
                message: message
            });
        } else {
            Ext.Viewport.mask({
                xtype: 'loadmask',
                message: message
            });
        }
    },

    unmaskBody: function () {
        var isNative = !!(window.cordova && cordova.plugin && cordova.plugin.pDialog);

        if (isNative) {
            cordova.plugin.pDialog.dismiss();
        } else {
            Ext.Viewport.unmask();
        }
    }

});
