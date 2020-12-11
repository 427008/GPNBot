/**
 * 
 */
Ext.define('GPNBot.navigation.Store', {
    extend: 'Ext.data.TreeStore',

    alias: 'store.navigationtree',

    fields: [{
        name: 'text'
    }],

    root: {
        expanded: true,
        children: [
            //{
            //    text: 'Мои события',
            //    iconCls: 'x-mi mi-home-outline',
            //    viewType: 'home',
            //    routeId: 'main/home',
            //    leaf: true
            //},
            {
                text: 'Главная',
                iconCls: 'x-mi mi-home-outline',
                //iconCls: 'x-mi mi-newspaper',
                viewType: 'home',
                routeId: 'main/home',
                leaf: true
            },
            //{
            //    text: 'Профиль',
            //    iconCls: 'x-mi mi-account-outline',
            //    viewType: 'profile',
            //    routeId: 'main/profile',
            //    leaf: true
            //},
            {
                //text: 'Чат бот',
                iconCls: 'x-mi mi-bell-outline',
                viewType: 'initiatives',
                routeId: 'main/initiatives',
                //rowCls: 'nav-tree-badge',
                leaf: true
            },
            //{
            //    text: 'Календарь',
            //    iconCls: 'x-mi mi-calendar-today',
            //    routeId: 'main/events',
            //    viewType: 'events',
            //    leaf: true
            //},
            //{
            //    text: 'Онлайн-консультация',
            //    iconCls: 'x-mi mi-whatsapp',
            //    link: 'https://wa.me/+79626900355',
            //    leaf: true
            //},
            //{
            //    text: 'Опросы',
            //    iconCls: 'x-mi mi-format-list-checks',
            //    routeId: 'main/polls',
            //    viewType: 'polls',
            //    leaf: true
            //},
            //{
            //    text: 'Рейтинги',
            //    iconCls: 'x-mi mi-chart-bar',
            //    routeId: 'main/ratings',
            //    viewType: 'ratings',
            //    leaf: true
            //},
            //{
            //    text: 'Бизнес-лаунж',
            //    iconCls: 'x-mi mi-briefcase',
            //    routeId: 'main/coworking',
            //    viewType: 'coworking',
            //    leaf: true
            //},
            //{
            //    text: 'Видео',
            //    iconCls: 'x-mi mi-video',
            //    routeId: 'main/videos',
            //    viewType: 'videos',
            //    leaf: true
            //},
            {
                text: 'Настройки',
                iconCls: 'x-mi mi-settings',
                viewType: 'properties',
                routeId: 'main/properties',
                leaf: true
            },
            {
                selectable: false,
                cls: 'x-separator',
                leaf: true
            },
            //{
            //    text: 'Сменить пользователя',
            //    iconCls: 'x-mi mi-account-switch',
            //    routeId: 'auth/logout',
            //    leaf: true
            //},
            {
                text: 'Выход',
                iconCls: 'x-mi mi-power',
                //iconCls: 'x-fa fa-question',
                routeId: 'auth/logout',
                leaf: true
            }
        ]
    }

});
