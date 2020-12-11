/**
 * 
 */
Ext.define('GPNBot.navigation.ViewController', {
    extend: 'Ext.app.ViewController',
    alias: 'controller.navigation',

    onProfileTap: function () {
        this.redirectTo('main/profile', { replace: true });
    },

    onNavigationItemClick: function (tree, info) {
        if (info.select) {
            this.view.setShowNavigation(false);
        }
    },

    onNavigationTreeSelectionChange: function (tree, node) {
        var me = this,
            to = node && (node.get('routeId') || node.get('viewType')),
            link = node && node.get('link'),
            main = me.view.getRefOwner();

        me.view.setShowNavigation(false);

        me.view.down('navigationlist').setSelection(null);

        if (to) {
            me.redirectTo(to, { replace: to === 'main/home' });
        }

        if (link) {
            window.open(link, '_system');
        }

    }
});
