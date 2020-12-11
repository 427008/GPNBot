/**
 * ServiceItem
 */
Ext.define('GPNBot.view.main.home.ServiceItem', {
    extend: 'Ext.Component',
    xtype: 'serviceitem',

    requires: [
    ],

    mixins: [
        'Ext.mixin.Responsive'
    ],

    config: {
        guid: undefined,
        //id: undefined,
        image: undefined,
        text: undefined,
        href: undefined
    },

    responsiveConfig: {
        'landscape': {
            style: 'height: calc(100% - 60px);width: calc(100vh - 40px);',
        },
        'portrait': {
            style: 'height: calc(100vw - 100px); width: calc(100% - 40px);',
        }
    },

    cls: 'x-service-item',
    maxWidth: 500,
    maxHeight: 400,

    getTemplate: function () {
        return [
            {
                reference: 'bodyElement',
                cls: 'x-body-el',
                children: [
                    {
                        reference: 'imageElement',
                        cls: 'x-image-el',
                    },
                    {
                        reference: 'textElement',
                        cls: 'x-text-el',
                        html: this.getText()
                    }
                ]
            }
        ];

    },

    listeners: {
        tap: 'onClick',
        element: 'bodyElement',
        scope: 'this'
    },

    onClick: function (event) {
        var me = this,
            initiatives = me.up('main').down('initiatives');

        event.stopEvent();
        //initiatives.setTitle(me.getText());
        Ext.getApplication().redirectTo('main/initiatives/' + me.getGuid());
    },

    updateText: function (value) {
        this.textElement.setHtml(value);
    },

    updateImage: function (value) {
        this.imageElement.setStyle('backgroundImage', 'url(' + value + ')')
    },

});
