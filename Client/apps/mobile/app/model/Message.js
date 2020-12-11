/**
 * 
 */
Ext.define('GPNBot.model.Message', {
    extend: 'Ext.data.Model',

    requires: [
        'Ext.data.identifier.Uuid'
    ],

    idProperty: 'guid',
    identifier: 'uuid',

    fields: [
        { name: 'guid', type: 'string' },
        { name: 'serviceGuid', type: 'string', allowNull: true },
        { name: 'questionCategory', type: 'int', allowNull: true },
        { name: 'sourceId', type: 'int', allowNull: true },
        { name: 'created', type: 'date', dateFormat: 'C', persist: false },
        { name: 'author', type: 'string', persist: false },
        { name: 'body', type: 'string' },
        { name: 'date', type: 'date', dateFormat: 'C' },
        { name: 'isMy', type: 'boolean' },
        { name: 'isSending', type: 'boolean' },
        { name: 'style', type: 'int', aloowNull: true, persist: false },
        { name: 'canClick', type: 'boolean', allowNull: true, persist: false },

        { name: 'isFile', type: 'boolean', allowNull: true },
        { name: 'fileName', type: 'string', allowNull: true },
        { name: 'fileType', type: 'string', allowNull: true },
        { name: 'lastModified', type: 'date', dateFormat: 'C', allowNull: true },
        { name: 'fileSize', type: 'int', allowNull: true },
        { name: 'validAnswer', type: 'boolean', allowNull: true },
        { name: 'commandValid', type: 'boolean', allowNull: true, persist: false },
        { name: 'src', type: 'string' },
        { name: 'command', type: 'int' }
        //{ name: 'isRead', type: 'boolean' }
    ],

    pageSize: 0,

    proxy: {
        type: 'rest',
        url: (Ext.APIUrl || '') + '/message',
        reader: {
            type: 'json',
            rootProperty: 'data',
            messageProperty: 'message'
        }
    }

});
