import type { DefineComponent } from 'vue';

declare type MenuItem = MenuItemWithParent | MenuItemWithLv1 | MenuItemChildren;

declare interface MenuItemBase {
    /** 用以显示在“菜单管理” -> “链接指向”。这里显示的是一级内容 */
    display: string,
    /** 显示图标，manual为false时，必填 */
    icon?: string,
    /** 为true时，不会在自动生成菜单时将其计算在内，默认为false */
    manual?: boolean, 
    /** 如果没有children项，则必须填写path */
    path?: string, 
    /** 该项的名称，不能与其它项 或者其它项的子项重复 */
    name: string, 
    /** 该项对应的组件，如果有children项，则对应的是layout组件 */
    component: DefineComponent<{}, {}, any> | (() => Promise<typeof import("*.vue")>), // 母版类
    /** 排序号 */
    order?: number,
    /** 类型 1：外链，2：Vue页面，3：上级菜单 */
    linkType? : number,
    /** 等效菜单的路径。如果命中了这条路由，那么菜单就会显示在另一项上 */
    as?: string,
    /** 是否所有人都允许访问，如果是则不需要身份验证 */
    allAccesss? : boolean,
    /** 子菜单项 */
    children?: Array<MenuItem>
};

declare interface MenuItemChildren extends MenuItemBase {
    /** 用以显示在“菜单管理” -> “链接指向”。这里显示的是一级内容 */
    display: string,
    /** 显示图标，manual为false时，必填 */
    icon?: string,
    /** 为true时，不会在自动生成菜单时将其计算在内，默认为false */
    manual?: boolean, 
    /** 如果没有children项，则必须填写path */
    path?: string, 
    /** 该项的名称，不能与其它项 或者其它项的子项重复 */
    name: string, 
    /** 该项对应的组件，如果有children项，则对应的是layout组件 */
    component: DefineComponent<{}, {}, any> | (() => Promise<typeof import("*.vue")>), // 母版类
    /** 排序号 */
    order?: number,
    /** 子菜单项 */
    children?: never
};

declare interface MenuItemWithLv1 extends MenuItemBase {
    /** 用以显示在“菜单管理” -> “链接指向”。这里显示的是一级内容 */
    display: string,
    /** 显示图标，manual为false时，必填 */
    icon: string,
    /** 该项的名称，不能与其它项 或者其它项的子项重复 */
    name: string, 
    /** 如果没有children项，则必须填写path */
    path: string, 
    /** 该项对应的组件，如果有children项，则对应的是layout组件 */
    component: DefineComponent<{}, {}, any> | (() => Promise<typeof import("*.vue")>), // 母版类
    /** 排序号 */
    order?: number,
    /** 子菜单项 */
    children?: never
};

declare interface MenuItemWithParent extends MenuItemBase {
    /** 用以显示在“菜单管理” -> “链接指向”。这里显示的是一级内容 */
    display: string,
    /** 显示图标，manual为false时，必填 */
    icon: string,
    /** 该项的名称，不能与其它项 或者其它项的子项重复 */
    name: string, 
    /** 该项对应的组件，如果有children项，则对应的是layout组件 */
    component: DefineComponent<{}, {}, any> | (() => Promise<typeof import("*.vue")>), // 母版类
    /** 排序号 */
    order?: number,
    /** 子菜单项 */
    children: Array<MenuItem>
};

export default MenuItem;