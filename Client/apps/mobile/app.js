/*
 * This file launches the application by asking Ext JS to create
 * and launch() the Application class.
 */
Ext.application({
    extend: 'GPNBot.Application',

    name: 'GPNBot',

    requires: [
    ]

});

window.DEBUG_CONNECTION = true;