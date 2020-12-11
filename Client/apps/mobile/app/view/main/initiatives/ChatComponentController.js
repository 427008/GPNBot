/**
 * 
 */
Ext.define('GPNBot.view.main.initiatives.ChatComponentController', {
    extend: 'Ext.app.ViewController',
    alias: 'controller.chat',

    requires: [
        'GPNBot.view.main.itemview.ItemView'
    ],

    //doLogout: function () {
    //    this.redirectTo('auth/logout', { replace: true });
    //},


    //doBack: function () {
    //    this.getViewModel().set('iid', 0);
    //},

    doChildTap: function (view, location) {
        var me = this,
            vm = me.getViewModel(),
            record = location.record;

        if (!record || !record.get('canClick')) return;

        me.view.up('main').setActiveItem({
            xtype: 'itemview',
            record: record
        });

        me.redirectTo('main/itemview');

        //me.view.sendMessage({
        //    command: 8,
        //    iid: record.get('iid'),
        //    body: record.get('iid'),
        //    sourceId: record.get('sourceId')
        //});


    },

    onCtrlEnterPress: function (event, field) {
        console.log(arguments);
        event.stopEvent();
        field.getTriggers().send.onClick(event);
        //field.getTriggers().send.el.dom.click();
    },

    onSendClick: function (field, trigger, event) {

        var messageText = field.getValue();

        messageText && messageText.trim();

        if (messageText && messageText.length) {
            this.view.sendMessage({
                body: messageText
            });
            field.clearValue();
        }
    },

    onToggleBars: function (button, pressed) {
        var me = this;

        me.view.setRevealed(!me.view.getRevealed());
    },

    onAudioClick: function (button, event) {
        var me = this;

        if (navigator.device && navigator.device.capture && navigator.device.capture.captureAudio) {
            event.stopEvent();
            me.view.setRevealed(false);
            navigator.device.capture.captureAudio(
                function (mediaFiles) {
                    var i, len;
                    for (i = 0, len = mediaFiles.length; i < len; i += 1) {
                        console.log(mediaFiles[i].fullPath, mediaFiles[i].localURL);
                        me.getFileEntry(mediaFiles[i].localURL);
                    }
                },
                function (error) {
                    var msg = 'An error occurred during capture: ' + error.code;
                    navigator.notification.alert(msg, null, 'Uh oh!');
                }
            );
            return false;
        }
    },

    onCameraClick: function (button, event) {
        var me = this;

        if (navigator.camera) {
            event.stopEvent();
            me.view.setRevealed(false);
            me.openCamera(me.setCamOptions(button.action === 'camera' ? Camera.PictureSourceType.CAMERA : Camera.PictureSourceType.PHOTOLIBRARY));
            return false;
        }
    },

    onCameraChange: function (button) {
        var me = this,
            buttonElement = button.buttonElement.dom,
            files = buttonElement.files;

        Ext.iterate(files, function (file) {
            me.readDataURL(file);
        });

        me.view.setRevealed(false);
    },

    doCommand: function (button) {
        var me = this,
            vm = me.getViewModel(),
            action = button.action;

        if (action.url) {
            window.open(action.url);
            return;
        }

        me.view.sendMessage({
            iid: vm.get('iid'),
            command: action.code,
            validAnswer: action.validAnswer,
            body: Ext.String.format(action.template, vm.get('iid')),
            commandText: action.commandText,
            sourceId: action.sourceId
        });
        vm.set('commands', 0);
    },

    doCommand2: function (button) {
        var me = this,
            vm = me.getViewModel(),
            action = button.action;

        me.view.sendMessage({
            iid: vm.get('iid'),
            command: action.code,
            body: Ext.String.format(action.template, vm.get('iid')),
            commandText: action.commandText,
            sourceId: action.sourceId
        });
        Ext.destroy(button.up('container[cls=x-checkbuttons]'));

    },

    privates: {

        setCamOptions: function (srcType) {
            var options = {
                // Some common settings are 20, 50, and 100
                quality: 50,
                destinationType: Camera.DestinationType.FILE_URI,
                // In this app, dynamically set the picture source, Camera or photo gallery
                sourceType: srcType,
                encodingType: Camera.EncodingType.JPEG,
                mediaType: Camera.MediaType.PICTURE,
                allowEdit: false,
                correctOrientation: true  //Corrects Android orientation quirks
            }
            return options;
        },

        openCamera: function (options) {
            var me = this;

            navigator.camera.getPicture(function cameraSuccess(imageUri) {

                me.getFileEntry(imageUri);

            }, function cameraError(error) {
                console.debug("Unable to obtain picture: " + error, "app");

            }, options);
        },

        readDataURL: function (file) {
            var me = this,
                //                view = me.down('dataview'),
                //                record = view.store.createModel({}),
                fileReader = new FileReader();

            console.log(file);

            fileReader.onload = function (event) {
                console.log('file loaded', file, fileReader.result);
                me.view.sendMessage({
                    isFile: true,
                    fileName: file.name,
                    fileType: file.type,
                    lastModified: file.lastModifiedDate,
                    fileSize: file.size,
                    body: file.name,
                    src: fileReader.result
                });

            };

            fileReader.onerror = function (event) {
                console.log('fileReader error', event)
            };

            fileReader.readAsDataURL(file);
        },

        getFileEntry: function (imgUri) {
            var me = this;

            window.resolveLocalFileSystemURL(
                imgUri,
                function success(fileEntry) {

                    console.log('fileEntry', fileEntry);
                    console.log("got file: " + fileEntry.fullPath);
                    fileEntry.file(function (file) {
                        me.readDataURL(file);
                    }, function (e) {
                        console.log('onErrorReadFile');
                        console.warn(e)
                    });

                },
                function () {
                    console.log('getFileEntry error', arguments)
                    // If don't get the FileEntry (which may happen when testing
                    // on some emulators), copy to a new FileEntry.
                    //createNewFileEntry(imgUri);
                }
            );
        }

    }

});

