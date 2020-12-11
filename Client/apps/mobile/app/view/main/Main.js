/**
 * 
 */
Ext.define('GPNBot.view.main.Main', {
    extend: 'Ext.navigation.View',
    alias: 'widget.main',

    requires: [
        'GPNBot.navigation.View',
        'GPNBot.view.main.MainController',
        'GPNBot.view.main.Toolbar',

        'GPNBot.view.main.home.Home',
        'GPNBot.view.main.initiatives.Initiatives',
        'GPNBot.view.main.properties.Properties'

    ],

    config: {
        title: undefined
    },

    controller: 'main',
    navigationBar: false,
    //useTitleForBackButtonText: true,
    //activeItem: 0,
    maxWidth: 600,

    items: [
        {
            xtype: 'maintoolbar',
            reference: 'maintoolbar',
            docked: 'top',
            shadow: false
        },

        {
            xtype: 'navigation',
            docked: 'left',
            reference: 'navigation'
        },

        {
            xtype: 'home'
        }

        //{
        //    xtype: 'initiatives'
        //}

    ],

    updateTitle: function (title) {
        var me = this,
            tb = me.lookup('maintoolbar');

        tb.setTitle(title);
    },

    setCurrentView: function (hashTag, guid) {
        hashTag = (hashTag || '').toLowerCase();

        var me = this,
            navigationTree = this.lookup('navigation').down('navigationlist'),
            store = navigationTree.getStore(),
            node = store.findNode('routeId', hashTag) || store.findNode('viewType', hashTag),
            item = me.child(hashTag),
            current = me.getActiveItem();
        
        if (!item) {
            if (node) {
                item = {
                    xtype: node.get('viewType')
                };
            } else {
                item = {
                    xtype: hashTag,
                    guid: guid || null
                };
            }
        }

        try {
            me.setActiveItem(item);

            if (node && !current || (node && node.get('viewType') !== current.xtype)) {
                me.setTitle(node.get('text'));
                navigationTree.suspendEvents();
                navigationTree.setSelection(node);
                navigationTree.resumeEvents();
            }

            current = me.getActiveItem();
            current.setGuid && current.setGuid(guid);

        } catch (e) {
            console.error(e);
        }
    },

    /**
     * Меняет кнопку справа в тулбраре
     * @param {String} action Кнопка, которая будет отображаться (save, back, notification).
     */
    changeToolbarButton: function (action) {
        var me = this,
            actionButtons = me.query('maintoolbar button[action]'),
            displayedButton = me.down('maintoolbar button[action=' + action + ']');

        actionButtons.forEach(function (item) {
            item.hide();
        });

        displayedButton && displayedButton.show();
    }


});
