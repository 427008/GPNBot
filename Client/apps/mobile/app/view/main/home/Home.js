/**
 * 
 */
Ext.define('GPNBot.view.main.home.Home', {
    extend: 'Ext.dataview.Component',
    xtype: 'home',

    requires: [
        'GPNBot.store.Service',
        'Ext.mixin.Responsive',
        'Ext.plugin.Responsive',
        'Ext.field.Search',
        'GPNBot.helpers.CurrentUser',
        'GPNBot.view.main.home.ServiceItem'
    ],

    mixins: [
        'Ext.mixin.Responsive'
    ],

    cls: 'x-home',
    scrollable: 'y',
    layout: { type: 'box' },

    store: 'service',

    viewModel: {
        data: {
            user: GPNBot.CurrentUser
        }
    },

    responsiveConfig: {
        'landscape': {
            layout: { vertical: false, align: 'start', pack: 'center', wrap: true }
        },
        'portrait': {
            layout: { vertical: true, align: 'stretch', pack: 'start', wrap: false }
        }
    },

    itemConfig: {
        viewModel: true,
        xtype: 'serviceitem',
        cls: 'x-service-item',
        bind: {
            guid: '{record.guid}',
            //id: '{record.id}',
            text: '{record.title}',
            image: '{record.image}',
            href: '{record.href}',
        }

    },

    items: [

        {
            xtype: 'container',
            cls: 'x-home-info',
            scrollDock: 'start',
            width: '100%',
            layout: {
                type: 'vbox',
                align: 'center'
            },
            items: [
                {
                    xtype: 'component',
                    bind: {
                        html: '{user.fio}'
                    }
                },
                {
                    xtype: 'component',
                    bind: {
                        html: 'Вам доступны следующие сервисы'
                    }
                }

            ]
        },

        {
            xtype: 'container',
            style: 'background-color: var(--base-color);',
            padding: 10,
            docked: 'bottom',
            items: {
                xtype: 'searchfield',
                ui: 'solo',
                placeholder: 'Поиск по сервисам',
                flex: 1

            }
        }
    ],

    listeners: {
        activate: function () {
            var main = this.up('main');
            main.changeToolbarButton(null);
            main.setTitle('Главная')
        }
    }

});
