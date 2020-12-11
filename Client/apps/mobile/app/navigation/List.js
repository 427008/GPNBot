/**
 * 
 */
Ext.define('GPNBot.navigation.List', {
    extend: 'Ext.list.Tree',
    alias: 'widget.navigationlist',

    requires: [
        'GPNBot.navigation.Store'
    ],

    scrollable: true,
    ui: 'nav',
    expanderFirst: false,
    expanderOnly: false,
    store: { type: 'navigationtree' },

    listeners: {
        itemclick: 'onNavigationItemClick',
        selectionchange: 'onNavigationTreeSelectionChange'
    },

    initialize: function () {
        var me = this,
            initiatives = me.getStore().findRecord('viewType', 'initiatives'),
            item = me.getItem(initiatives);

        me.callParent(arguments);

        //console.log(item)
        item.setHidden(true);

        //profileTreelistItem.setStyle('display: none;');

        //Ext.on('clearbadgetext', function () {
        //    me.getStore().findRecord('viewType', 'notification').set('rowCls', '');
        //});

        //Ext.on('setbadgetext', function () {
        //    me.getStore().findRecord('viewType', 'notification').set('rowCls', 'nav-tree-badge');
        //});
    }

});
