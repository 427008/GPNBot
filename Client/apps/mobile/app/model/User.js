/**
 * 
 */
Ext.define('GPNBot.model.User', {
    extend: 'Ext.data.Model',
    requires: [
        'SU.data.field.Json'
    ],

    idProperty: 'guid',

    fields: [
        { name: 'guid', type: 'string', persist: false },
        { name: 'fio', type: 'string', persist: false },
        { name: 'position', type: 'string', persist: false },
        { name: 'employmentDate', type: 'date', persist: false }
    ],

    clean: function () {
        var me = this,
            obj = {};

        Object.keys(me.fieldsMap).forEach(function (name) {
            obj[name] = null;
        });

        me.set(obj);
        me.commit();
    }

});
