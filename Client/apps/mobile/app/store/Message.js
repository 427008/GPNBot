/**
 * Список сообщений чата с поддержкой автоматической перезагрузки.
 */
Ext.define('GPNBot.store.Message', {
    extend: 'Ext.data.Store',
    alias: 'store.message',

    requires: [
        'Ext.data.proxy.Rest',
        'GPNBot.model.Message'
    ],

    config: {
        /**
         * @cfg {String} serviceGuid
         * GUID чата
         */
        serviceGuid: undefined
    },

    model: 'GPNBot.model.Message',
    remoteFilter: true,
    //remoteSort: true,
    pageSize: 30,
    autoLoad: false,

    sorters: [
        {
            property: 'id',
            direction: 'ASC'
        }
    ],


    grouper: {
        sortProperty: 'id',
        direction: 'ASC',
        groupFn: function (record) {
            var date = Ext.Date.clearTime(record.get('date'), true),
                today = Ext.Date.clearTime(new Date());

            if (!Ext.isDate(date)) {
                date = Ext.Date.clearTime(new Date());
            }

            //if(!record.get('isRead')){
            //    return 'Непрочитанные сообщения';
            //} else
            if (Ext.Date.isEqual(date, today)) {
                return 'Сегодня';
            } else if (Ext.Date.isEqual(date, Ext.Date.subtract(today, Ext.Date.DAY, 1))) {
                return 'Вчера'
            } else {
                return Ext.Date.format(date, 'l, j M Y');
            }
        }
    },

    listeners: {
        beforeload: 'onStoreBeforeLoad',
        load: 'onStoreLoad',
        clear: 'onStoreClear'
    },

    constructor: function () {
        var me = this;

        me.callParent(arguments);
        //Ext.WSConnection.on({
        //    message: 'doWSMessage',
        //    scope: me
        //});
    },

    destroy: function () {
        this.clearTimeout();
        //Ext.WSConnection.on({
        //    message: 'doWSMessage',
        //    scope: me
        //});
        this.callParent(arguments);
    },

    //doWSMessage: function (ws, data) {
    //    var me = this,
    //        records;

    //    console.log(data);

    //    if (data.dst !== 'message') return;

    //    if (data.cmd === 'newrec') {
    //        records = Ext.Array.from(data.data);
    //        for (var i = 0, len = records.length; i < len; i++) {
    //            me.add(records[i]);
    //        }
    //    }
    //},

    clearTimeout: function () {
        var me = this;

        //if (me.reloadTimer) {
        me.reloadTimer = clearTimeout(me.reloadTimer);
        //}
        //me.reloadTimer = null;
    },

    prepareLastTimeFilter: function () {
        var me = this;

        //console.log('lastTime: %o, %o', me.lastTime, Ext.Date.format(me.lastTime, 'C'))

        me.clearTimeout();

        if (me.lastTime) {
            me.addFilter({
                property: 'id',
                operator: 'gt',
                value: me.lastTime
            }, true);
        } else {
            me.removeFilter('id', true);
        }
    },

    doReload: function () {
        var me = this;

        me.prepareLastTimeFilter();
        Ext.isOnline() && me.reload({ addRecords: true });
    },

    onStoreBeforeLoad: function (store, operation) {
        var me = this;

        me.clearTimeout();
    },

    onStoreLoad: function (store, records, success) {
        var me = this,
            lastRec;

        if (success) {
            lastRec = me.getAt(me.getCount() - 1);
            me.lastTime = lastRec && lastRec.data.id;
        }

        me.clearTimeout();
        me.reloadTimer = setTimeout(Ext.bind(me.doReload, me), 5 * 1000);
    },

    onStoreClear: function (store) {
        this.clearTimeout();
    },

    updateServiceGuid: function (value) {
        var me = this,
            proxy = me.getProxy();

        proxy.setExtraParams(value ? { serviceGuid: value } : {});
        me.doReload();
    }

});
