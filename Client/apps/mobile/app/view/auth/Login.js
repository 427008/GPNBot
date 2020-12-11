/**
 * 
 */
Ext.define('GPNBot.view.auth.Login', {
    extend: 'Ext.form.Panel',
    alias: 'widget.authlogin',

    requires: [
        'Ext.layout.HBox',
        'Ext.form.Panel',
        'Ext.field.Password',
        'Ext.field.Email',
        'Ext.field.Text',
        'Ext.field.Checkbox',
        'Ext.data.validator.Email'
    ],

    //controller: 'auth',
    padding: '20 40',
    layout: { type: 'vbox', align: 'stretch', pack: 'start' },
    reference: 'authlogin',
    //maxWidth: 500,

    keyMap: {
        ENTER: 'onLoginEnter'
    },

    defaults: {
        labelAlign: 'top',
        ui: 'faded'
    },

    items: [
        {
            xtype: 'emailfield',
            label: 'Логин',
            placeholder: 'Введите логин',
            required: true,
            name: 'login'
        },

        {
            xtype: 'passwordfield',
            label: 'Пароль',
            placeholder: 'Введите пароль',
            required: true,
            name: 'password'
        },

        {
            xtype: 'checkbox',
            //ui: 'faded',
            bodyAlign: 'start',
            boxLabel: 'Запомнить меня',
            style: 'margin: 20px auto 0px;border:none;position:relative;right:3%;',
            name: 'remember',
            checked: true
        },

        {
            xtype: 'container',
            layout: { type: 'hbox', align: 'start', pack: 'center' },
            padding: '20 0',
            items: [
                {
                    xtype: 'button',
                    ui: 'action round',
                    minWidth: 250,
                    text: 'ВОЙТИ',
                    handler: 'onLogin',
                    ripple: false
                }
            ]
        }

    ]
});
