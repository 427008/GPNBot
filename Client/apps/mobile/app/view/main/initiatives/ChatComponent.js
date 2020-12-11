/**
 * 
 */
Ext.define('GPNBot.view.main.initiatives.ChatComponent', {
    extend: 'Ext.dataview.List',
    alias: 'widget.chatcomponent',

    requires: [
        'Ext.field.FileButton',
        'Ext.Audio',
        //'Ext.layout.overflow.Scroller',
        'GPNBot.store.Message',
        'SU.widgets.Avatar',
        'GPNBot.view.main.initiatives.ChatComponentController'
    ],

    config: {
        /**
         * @cfg {String} guid require
         * GUID чата
         */
        guid: {
            $value: undefined,
            lazy: true
        },

        questionCategory: {
            $value: undefined,
            lazy: true
        },

        /**
         * @cfg {Boolean} revealed
         * Состояние раскрытия медиа кнопок
         */
        revealed: {
            $value: false,
            lazy: true
        }

    },

    viewModel: {
        formulas: {
            isNativeCam: {
                get: function () {
                    return !!navigator.camera;
                }
            }
        }
    },

    controller: 'chat',
    store: { type: 'message' },
    grouped: true,
    groupHeader: { tpl: '{html}' },
    scrollToTopOnRefresh: false,
    scrollable: true,
    emptyText: 'Нет сообщений',
    userCls: 'x-chatcomponent',
    //masked: false,
    selectable: false,
    revealedCls: 'x-revealed',

    //infinite: true,
    //pinHeaders: true,
    //pinnedHeader: { xtype: 'rowheader' },

    itemConfig: {
        viewModel: true,

        xtype: 'container',
        margin: '0 0 10',
        items: [
            {
                xtype: 'container',
                layout: { type: 'hbox', align: 'start' },
                bind: {
                    cls: 'x-im-message {record.command == 0 ? "" : "x-cmd"} {record.isMy ? "self" : "other"} {record.commandValid === true ? "x-valid" : record.commandValid === false ? "x-invalid" : ""} {record.isRead ? "x-readed" : ""} {record.isSending ? "x-im-sending" : ""}'
                },
                items: [

                    // Аватарка
                    {
                        xtype: 'avatar',
                        ui: 'plain',
                        proportions: 56,
                        image: 'resources/images/icons/robot-icon.png'
                    },

                    // Контейнер сообщения
                    {
                        xtype: 'container',
                        flex: 1,
                        cls: 'info',
                        //layout: { type: 'hbox' },
                        items: [

                            // Время
                            {
                                xtype: 'component',
                                cls: 'x-msg-date',
                                bind: {
                                    html: '{record.date:date("H:i")}'
                                }
                            },

                            // Пузырь
                            {
                                xtype: 'container',
                                cls: 'bubble',
                                //flex: 1,
                                items: [
                                    {
                                        xtype: 'component',
                                        cls: 'x-author',
                                        bind: {
                                            html: '{record.author:htmlEncode()}'
                                        }
                                    },

                                    // Текст сообщения
                                    {
                                        xtype: 'component',
                                        bind: {
                                            html: '{record.body:nl2br()}'
                                        }
                                    },

                                    // Файл
                                    {
                                        xtype: 'container',
                                        cls: 'x-fileinfo',
                                        hidden: true,
                                        bind: {
                                            hidden: '{!record.isFile}'
                                        },
                                        items: [

                                            // Размер файла
                                            {
                                                xtype: 'component',
                                                cls: 'x-filesize',
                                                bind: {
                                                    html: '{record.fileSize:fileSize()}',
                                                    hidden: '{!record.fileSize}'
                                                }
                                            },

                                            // Картинка
                                            {
                                                xtype: 'img',
                                                mode: 'image',
                                                maxHeight: 300,
                                                bind: {
                                                    src: '{record.src}',
                                                    hidden: '{!record.isFile || record.fileType:substr(0, 6) != "image/"}'
                                                },
                                                listeners: {
                                                    load: function (me) {
                                                        var img = new Image(),
                                                            w, h, w1, h1, d;

                                                        img.src = me.getSrc();
                                                        w = img.width;
                                                        h = img.height;
                                                        delete img;

                                                        me.setWidth(w).setHeight(h);
                                                        w1 = me.parent.el.getWidth() - 20;
                                                        h1 = me.parent.el.getHeight() - 17;
                                                        d = Math.min(w1 / w, h1 / h);
                                                        //console.log(w, h, w1, h1, w1 / w, h1 / h, d, w * d, h * d);

                                                        me.setWidth(w * d);
                                                        me.setHeight(h * d);
                                                    }
                                                }
                                            },

                                            // Аудио
                                            {
                                                xtype: 'audio',
                                                minWidth: '70vw',
                                                bind: {
                                                    url: '{record.src}',
                                                    hidden: '{!record.isFile || record.fileType:substr(0, 6) != "audio/"}'
                                                }
                                            }

                                        ]
                                    }

                                ]

                            }
                        ]
                    },
                    {
                        xtype: 'component',
                        cls: 'x-info-icon'
                    }
                ]
            }
        ]
    },

    items: [

        // Тулбар для кнопок управления ботом
        {
            xtype: 'container',
            reference: 'cmd-buttons',
            cls: 'x-cmd-buttons',
            scrollDock: 'end',
            padding: '0 10',
            layout: {
                type: 'hbox',
                align: 'stretch',
                pack: 'space-around',
                wrap: true
            },
            defaults: {
                //width: '49%',
                //margin: '10 0',
                //    style: 'margin-top: 10px;margin-bottom: 10px;'
            },
            bind: {
                hidden: '{!commands}'
            }
        },

        {
            xtype: 'container',
            docked: 'bottom',

            items: [
                // Тулбар для кнопок медиа
                {
                    xtype: 'container',
                    reference: 'media-buttons',
                    weight: 200,
                    hidden: true,

                    bind: {
                        hidden: '{!revealed}'
                    },

                    defaults: {
                        minWidth: 85,
                        style: 'margin-top: 10px;margin-bottom: 10px;',
                        ui: 'bot'
                    },

                    items: [
                        {
                            xtype: 'button',
                            iconCls: 'x-mi mi-microphone',
                            text: 'Звук',
                            action: 'cound',
                            listeners: {
                                tap: 'onAudioClick'
                            }
                        },
                        {
                            xtype: 'filebutton',
                            accept: 'image',
                            capture: 'camcorder',
                            iconCls: 'x-mi mi-camera',
                            text: 'Камера',
                            action: 'camera',
                            bind: {
                                disabled: '{!isNativeCam}'
                            },
                            listeners: {
                                tap: 'onCameraClick',
                                change: 'onCameraChange'
                            }
                        },

                        {
                            xtype: 'filebutton',
                            accept: 'image/*',
                            //capture: 'capture',
                            iconCls: 'x-mi mi-image',
                            text: 'Галерея',
                            action: 'gallery',
                            listeners: {
                                tap: 'onCameraClick',
                                change: 'onCameraChange'
                            }
                        }
                    ]
                },

                // Тулбар с полем набора сообщения и кнопкой отправки
                {
                    xtype: 'container',
                    style: 'background-color: var(--base-color);',
                    padding: '10 10 8',

                    items: [
                        {
                            xtype: 'textareafield',
                            placeholder: 'Введите текст сообщения',
                            reference: 'textmessage',
                            maxHeight: 150,
                            ui: 'solo',
                            cls: 'x-field-chat',
                            autoGrow: true,
                            triggers: {
                                plus: {
                                    side: 'left',
                                    focusOnTap: false,
                                    cls: 'x-trigger-plus',
                                    handler: 'onToggleBars'
                                },
                                send: {
                                    cls: 'x-trigger-send',
                                    focusOnTap: false,
                                    handler: 'onSendClick'
                                }
                            },
                            keyMap: {
                                'CmdOrCtrl+ENTER': 'onCtrlEnterPress'
                            }
                        }
                    ]
                }
            ]
        },
    ],

    listeners: {
        storeadd: { fn: 'onAfterStoreAdd', buffer: 10, scope: 'this' },
        childtap: 'doChildTap'
    },

    initialize: function () {
        var me = this;

        me.callParent(arguments);

        me.relayEvents(me.getStore(), ['load', 'add'], 'store');

        Ext.WSConnection.on({
            message: 'doWSMessage',
            scope: me
        });

    },

    destroy: function () {
        var me = this;

        Ext.destroy(me.store);

        Ext.WSConnection.on({
            message: 'doWSMessage',
            scope: me
        });

        me.callParent(arguments);
    },

    updateGuid: function (newValue, oldValue) {
        var me = this,
            store = me.getStore();

        store.setServiceGuid(newValue);
        me.getViewModel().set('guid', newValue);
        //if (!oldValue)
        me.firstLoad = true;
        me.isSended = false;
        me.setQuestionCategory(null);
    },

    updateRevealed: function (newValue, oldValue) {
        var me = this,
            trigger = me.lookup('textmessage').getTriggers().plus;

        if (newValue) {
            trigger.addCls(me.revealedCls);
        }
        else {
            trigger.removeCls(me.revealedCls);
        }

        me.updateState();
        me.getViewModel().set('revealed', newValue);
    },

    privates: {

        /**
         * @property {boolean} firstLoad
         * Флаг первой загрузки
         * @private
         */
        firstLoad: undefined,
        isSended: undefined,

        startDate: new Date(),

        doWSMessage: function (ws, data) {
            var me = this,
                records;

            console.log(data);

            if (data.dst !== 'message') return;

            if (data.cmd === 'newrec') {
                records = Ext.Array.from(data.data);
                for (var i = 0, len = records.length; i < len; i++) {
                    me.store.add(records[i]);
                }
                me.updateState();
                me.scrollBootom();
            }
        },

        /**
         * Прячем маску загрузки при повторных обновлениях
         * @private 
         */
        handleBeforeLoad: function () {
            var me = this;

            me.callParent(arguments);

        },

        /**
         * При обновлении данных
         * 
         * - обновление состояния
         * - скроллинг в конец списка
         */
        onStoreLoad: function (store, records, success, options) {
            var me = this;

            me.callParent(arguments);

            //if (!success && me.firstLoad && options.error) {
            //    Ext.Msg.alert(
            //        'Что-то пошло не так',
            //        options.error,
            //        function () {
            //            Ext.getApplication().redirectTo('auth/logout');
            //        }
            //    );
            //}
            //console.log('onStoreLoad', (new Date().getTime() - me.startDate.getTime()) / 1000 )

            me.updateState();
            me.scrollBootom(me.firstLoad, !me.firstLoad);
            me.firstLoad = false;

            if (me.getLoadingText()) {
                me.setLoadingText(false);
            }

        },

        /**
         * При добавлении записи 
         * 
         * - устанавливается пометка непрочитанного сообщения
         * - скроллинг в конец списка
         * 
         * @param {Ext.data.Store} store хранилище данных
         * @param {Ext.data.Model[]} records добавленные записи
         * @param {Number} index индекс первой добывленной записи
         */
        onAfterStoreAdd: function (store, records, index) {
            this.scrollBootom(true);
        },

        /**
         * Скроллим вниз к новым сообщениям
         * @param {Boolean} isBottom
         */
        scrollBootom: function (isBottom, animate) {
            var me = this,
                scroller = me.getScrollable();

            isBottom = isBottom || scroller.position.y >= scroller.getSize().y - scroller.component.element.getHeight() - 84;

            // Скроллим вниз к новой записи, если скролл уже был в самом низу
            // При смещенном скроле даем спокойно читать то что видим
            if (isBottom) {
                Ext.defer(function () {
                    scroller.scrollTo(0, Infinity, animate !== false);
                }, animate !== false ? 50 : 150);
            }
        },

        /**
         * Подготовка и отправка сообщения
         * @param {Object} options
         */
        sendMessage: function (options, callback, scope) {
            var me = this,
                scroller = me.getScrollable(),
                record;

            record = me.store.createModel(Ext.apply(options, {
                author: GPNBot.CurrentUser.getData().fio,
                date: new Date(),
                isMy: true,
                isSending: true
            }));

            if (me.getGuid()) {
                record.set('serviceGuid', me.getGuid());
            }

            if (me.getQuestionCategory()) {
                record.set('questionCategory', me.getQuestionCategory());
            }
            
            console.log(record);

            me.store.add(record);
            me.sendRecord(record);
        },

        /**
         * Отправка сообщения по `http` или `webSocket`
         * @param {Ext.data.Model} record -
         */
        sendRecord: function (record, callback, scope) {
            var me = this;

            me.store.clearTimeout();
            record.save({
                callback: function (rec, opt, success) {
                    if (success) {
                        me.refresh();
                        console.log(record.data);
                        Ext.defer(function () {
                            me.store.doReload();
                            me.isSended = true;
                        }, 1000);
                    }
                    Ext.callback(callback, scope || me, [record, success]);
                }
            })
        },

        /**
         * Обновление состояния по последнему сообщению бота
         */
        updateState: function () {
            var me = this,
                vm = me.getViewModel(),
                botMsg = me.findLastBotMessage();

            vm.set('iid', botMsg && botMsg.get('iid'));
            me.setQuestionCategory(botMsg && botMsg.get('questionCategory'));

            me.updateCmdButton(me.store.last());

            if (botMsg && me.isSended && me.getGuid() !== botMsg.get('serviceGuid')) {
                Ext.defer(function () {
                    me.up('initiatives').setGuid(botMsg.get('serviceGuid'));
                }, 250);
            }
        },

        /**
         * Обновление кнопок управления ботом
         * @param {Object} options
         */
        updateCmdButton: function (record) {
            var me = this,
                tb = me.lookup('cmd-buttons'),
                oldButtons = tb.query('button[action]'),
                options = record && record.get('commands'),
                item;

            if (!record) return;

            me.suspendEvents();

            if (!record || options) {
                if (oldButtons) {
                    Ext.iterate(oldButtons, function (item) {
                        Ext.destroy(item);
                    });
                }
            }

            Ext.iterate(me.query('container[cls=x-checkbuttons]'), function (cfg) { Ext.destroy(cfg); });


            if (record.get('style') !== 1) {
                if (options) {
                    Ext.iterate(options, function (item, index) {
                        tb.add({
                            xtype: 'button',
                            ui: 'bot',
                            text: item.title,
                            action: item,
                            handler: 'doCommand'
                        });
                    });
                }

                me.getViewModel().set('commands', tb.getItems().getCount());
            } else {
                if (options) {
                    item = me.itemFromRecord(record);

                    if (!item.down('container[cls=x-checkbuttons]')) {
                        tb = item.add({
                            xtype: 'container',
                            cls: 'x-checkbuttons',
                            layout: { type: 'hbox', align: 'start', pack: 'space-around' },
                            docked: 'bottom',
                            maxWidth: '90%'
                        });
                        Ext.iterate(options, function (cfg, index) {
                            tb.add({
                                xtype: 'container',
                                layout: { type: 'vbox', align: 'center' },
                                margin: 5,
                                items: [
                                    {
                                        xtype: 'component',
                                        cls: 'x-cmd-icon x-cmd-icon-' + cfg.code,
                                        listeners: {
                                            tap: function (event) {
                                                event.stopEvent();
                                                this.component.up().down('button').onClick();
                                            },
                                            element: 'element'
                                        }
                                    },
                                    {
                                        xtype: 'button',
                                        ui: 'bot',
                                        margin: 0,
                                        text: cfg.title,
                                        width: 90,
                                        action: cfg,
                                        handler: 'doCommand2'
                                    }
                                ]
                            });
                        });
                    }
                }
            }

            me.resumeEvents();
        },

        /**
         * Последнее сообщение бота
         */
        findLastBotMessage: function () {
            var me = this,
                store = me.getStore(),
                index = store.getCount() - 1,
                rec;

            for (; index >= 0; index--) {
                rec = store.getAt(index);
                if (!rec.get('isMy')) return rec;
            }
        }

    }
});
