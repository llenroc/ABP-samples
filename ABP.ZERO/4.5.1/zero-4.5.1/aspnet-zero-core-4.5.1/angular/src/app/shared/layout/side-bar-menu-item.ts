export class SideBarMenuItem {
    name: string = '';
    permissionName: string = '';
    icon: string = '';
    route: string = '';
    items: SideBarMenuItem[];

    constructor(name: string, permissionName: string, icon: string, route: string, items?: SideBarMenuItem[]) {
        this.name = name;
        this.permissionName = permissionName;
        this.icon = icon;
        this.route = route;

        if (items === undefined) {
            this.items = [];    
        } else {
            this.items = items;
        }        
    }
}