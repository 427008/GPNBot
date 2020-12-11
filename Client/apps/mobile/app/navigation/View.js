/**
 * 
 */
Ext.define('GPNBot.navigation.View', {
    extend: 'Ext.Container',
    alias: 'widget.navigation',

    requires: [
        'GPNBot.helpers.CurrentUser',
        'GPNBot.navigation.List',
        'GPNBot.navigation.ViewController',
        'SU.widgets.Avatar',
        'Ext.layout.Fit',
        'Ext.app.ViewModel'
    ],

    config: {
        showNavigation: false
    },

    controller: 'navigation',
    layout: 'fit',

    viewModel: {
        data: {
            version: Ext.versions.gpn.version
        }
    },

    items: [

        // Профиль
        {
            xtype: 'container',
            docked: 'top',
            cls: 'x-user-panel',
            layout: { type: 'vbox', align: 'stretch' },
            items: [
                //
                {
                    xtype: 'avatar',
                    ui: 'plain',
                    cls: 'x-user-info-photo',
                    alignSelf: 'center',

                    //listeners: {
                    //    click: 'onProfileTap',
                    //    element: 'element'
                    //},

                    bind: {
                        userName: '{user.fio}',
                        //image: '{user.photo}'
                    }
                },

                //
                {
                    xtype: 'component',
                    cls: 'x-user-info',
                    //listeners: {
                    //    click: 'onProfileTap',
                    //    element: 'element'
                    //},
                    bind: {
                        html:
                            '<div class="x-user-info-name">' +
                            '<span>{user.fio}</span>' +
                            '<span>{user.position}</span>' +
                            '</div>'
                    }

                }
            ]
        },

        // Навигация
        {
            xtype: 'navigationlist',
            scrollable: 'y'
        },

        // версия
        {
            xtype: 'component',
            docked: 'bottom',
            cls: 'x-version',
            bind: {
                html: '{version}'
            }

        }
        //{
        //    xtype: 'container',
        //    cls: 'x-social-info',
        //    docked: 'bottom',
        //    items: [
        //        {
        //            xtype: 'button',
        //            iconCls: 'x-mi mi-vk',
        //            cls: 'x-social-info-item',
        //            handler: function () {
        //                window.open('https://vk.com/crpp_ru', '_system');
        //            }
        //        },
        //        {
        //            xtype: 'button',
        //            iconCls: 'x-mi mi-facebook',
        //            cls: 'x-social-info-item',
        //            handler: function () {
        //                window.open('https://www.facebook.com/crpp.ru', '_system');
        //            }
        //        },
        //        {
        //            xtype: 'button',
        //            iconCls: 'x-mi mi-instagram',
        //            cls: 'x-social-info-item',
        //            handler: function () {
        //                window.open('https://www.instagram.com/crpp.ru', '_system');
        //            }
        //        },
        //        {
        //            xtype: 'button',
        //            iconCls: 'x-mi mi-twitter',
        //            cls: 'x-social-info-item',
        //            handler: function () {
        //                window.open('https://twitter.com/crpp_ru', '_system'); }
        //        },
        //        {
        //            xtype: 'button',
        //            iconCls: 'x-mi mi-odnoklassniki',
        //            cls: 'x-social-info-item',
        //            handler: function () {
        //                window.open('https://ok.ru/group/53965952909490', '_system'); }
        //        },
        //        {
        //            xtype: 'button',
        //            iconCls: 'x-mi mi-youtube-play',
        //            cls: 'x-social-info-item',
        //            handler: function () { window.open('https://www.youtube.com/channel/UC-jF9pa9GlxyT9mvI4YNXAQ', '_system'); }
        //        }
        //    ]
        //}
    ],

    slideOutCls: 'x-nav-slid-out',

    initialize: function () {
        var me = this;

        me.callParent(arguments);
        me.getViewModel().set('user', GPNBot.CurrentUser);
    },

    onRender: function () {
        var me = this,
            view;

        me.callParent(arguments);


        view = me.getParent();

        view.remove(me, false);
        me.addCls(['x-floating', 'x-nav-floated', me.slideOutCls]);
        me.setScrollable(true);
        me.getRefOwner = function () {
            return view;
        };

        Ext.getBody().appendChild(me.element);

    },

    updateShowNavigation: function (showNavigation, oldValue) {
        var me = this,
            mask = me.mask;

        if (oldValue !== undefined) {

            if (showNavigation) {
                me.mask = mask = Ext.Viewport.add({
                    xtype: 'mask',
                    cls: 'x-nav-mask'
                });

                mask.element.on({
                    tap: 'onToggleNavigationSize',
                    scope: me,
                    single: true
                });
            } else if (mask) {
                me.mask = Ext.destroy(mask);
            }

            me.toggleCls(me.slideOutCls, !showNavigation);
        }
    },

    onToggleNavigationSize: function () {
        this.setShowNavigation(!this.getShowNavigation());
    }

});
