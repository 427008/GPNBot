/**
 * 
 */
Ext.define('GPNBot.helpers.WebSocketConnection', {

    alternateClassName: ['GPNBot.WSConnection', 'Ext.WSConnection'],
    extend: 'SU.WebSocket',

    singleton: true,

    /**
     * Обработка события onmessage WebSocket.
     * @param {Ext.event.Event} e Событие WebSocket
     * @protected
     */
    handleInstanceMessage: function (e) {
        var me = this,
            json;

        try {
            json = JSON.parse(e.data);
            if (json.type = 'event') {
                me.fireEvent('message', me, json);
            } else {
                // conrtoll commad
            }
        } catch (e) {
            //
        }
    },

});
