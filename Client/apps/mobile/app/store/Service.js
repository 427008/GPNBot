/**
 * Список сообщений чата с поддержкой автоматической перезагрузки.
 */
Ext.define('GPNBot.store.Service', {
    extend: 'Ext.data.Store',
    singleton: true,
    //alias: 'store.service',
    storeId: 'service',

    requires: [
        'GPNBot.model.Service'
    ],

    model: 'GPNBot.model.Service',
    //remoteFilter: true,
    //remoteSort: true,
    pageSize: 0,
    //autoSync: true,
    //autoLoad: true,

    //sorters: [
    //    {
    //        property: 'date',
    //        direction: 'ASC'
    //    }
    //],

});
