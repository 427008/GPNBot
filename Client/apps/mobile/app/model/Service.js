/**
 * 
 */
Ext.define('GPNBot.model.Service', {
    extend: 'Ext.data.Model',

    requires: [
        'Ext.data.proxy.Rest'
    ],

    idProperty: 'guid',

    fields: [
        { name: 'guid', type: 'string', persist: false },
        { name: 'id', type: 'int', persist: false  },
        { name: 'title', type: 'string', persist: false },
        { name: 'image', type: 'string', persist: false },
        { name: 'href', type: 'string', persist: false }
    ],

    pageSize: 0,

    proxy: {
        type: 'rest',
        url: (Ext.APIUrl || '') + '/service',
        reader: {
            type: 'json',
            rootProperty: 'data',
            messageProperty: 'message'
        }
    }

});
