import { Component, Injectable, Injector, ViewChild, OnInit, AfterViewInit, ViewEncapsulation } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { AppSessionService } from '@shared/common/session/app-session.service';
import { QuickSideBarChat } from 'app/shared/layout/chat/QuickSideBarChat';
import { DomHelper } from '@shared/helpers/DomHelper';
import {
    FriendshipServiceProxy, ChatServiceProxy, CommonLookupServiceProxy, ProfileServiceProxy,
    FriendDto, UserLoginInfoDto, BlockUserInput, UnblockUserInput, ChatMessageDto, ChatMessageDtoReadState,
    MarkAllUnreadMessagesOfUserAsReadInput, NameValueDto, FindUsersInput, CreateFriendshipRequestInput,
    CreateFriendshipRequestByUserNameInput, FriendDtoState, ChatMessageDtoSide
} from '@shared/service-proxies/service-proxies';
import { ChatFriendDto } from './ChatFriendDto';
import { CommonLookupModalComponent } from '@app/shared/common/lookup/common-lookup-modal.component';
import { LocalStorageService } from '@shared/utils/local-storage.service'
import { ChatSignalrService } from './chat-signalr.service';
import { AppChatMessageReadState, AppChatSide, AppFriendshipState } from '@shared/AppEnums';

import * as moment from 'moment';
import * as _ from 'lodash';

@Component({
    templateUrl: './chat-bar.component.html',
    selector: 'chat-bar',
    styleUrls: ['./chat-bar.component.less'],
    encapsulation: ViewEncapsulation.None
})
export class ChatBarComponent extends AppComponentBase implements OnInit, AfterViewInit {

    constructor(injector: Injector,
        private _appSessionService: AppSessionService,
        private _friendshipService: FriendshipServiceProxy,
        private _chatService: ChatServiceProxy,
        private _commonLookupService: CommonLookupServiceProxy,
        private _localStorageService: LocalStorageService,
        private _chatSignalrService: ChatSignalrService,
        private _profileService: ProfileServiceProxy,
        private _quickSideBarChat: QuickSideBarChat) {
        super(injector);
    }

    @ViewChild('userLookupModal') userLookupModal: CommonLookupModalComponent;
    $_chatMessageInput: JQuery;

    friendDtoState: typeof AppFriendshipState = AppFriendshipState;

    friends: ChatFriendDto[];
    currentUser: UserLoginInfoDto = this._appSessionService.user;
    profilePicture: string = "/assets/common/images/default-profile-picture.png";
    chatMessage: string = '';

    tenantToTenantChatAllowed: boolean = false;
    tenantToHostChatAllowed: boolean = false;
    interTenantChatAllowed: boolean = false;
    sendingMessage: boolean = false;
    loadingPreviousUserMessages: boolean = false;
    userNameFilter: string = '';
    serverClientTimeDifference: number = 0;
    isMultiTenancyEnabled: boolean = this.multiTenancy.isEnabled;

    _isOpen: boolean;
    set isOpen(newValue: boolean) {
        if (newValue === this._isOpen) {
            return;
        }

        this._localStorageService.setItem('app.chat.isOpen', newValue);
        this._isOpen = newValue;

        if (newValue) {
            this.markAllUnreadMessagesOfUserAsRead(this.selectedUser);
        }
        
        this.adjustNotifyPosition();
    }
    get isOpen(): boolean {
        return this._isOpen;
    }

    _pinned: boolean = false;
    set pinned(newValue: boolean) {
        if (newValue === this._pinned) {
            return;
        }

        this._pinned = newValue;
        this._localStorageService.setItem('app.chat.pinned', newValue);
    }
    get pinned(): boolean {
        return this._pinned;
    }

    _selectedUser: ChatFriendDto = new ChatFriendDto();
    set selectedUser(newValue: ChatFriendDto) {
        if (newValue === this._selectedUser) {
            return;
        }

        this._selectedUser = newValue;

        //NOTE: this is a fix for localForage is not able to store user with messages array filled
        if (newValue.messages) {
            newValue.messages = [];
            newValue.messagesLoaded = false;
        }
        this._localStorageService.setItem('app.chat.selectedUser', newValue);
    }
    get selectedUser(): ChatFriendDto {
        return this._selectedUser;
    }

    ngOnInit(): void {
        this.init();
    }

    getShownUserName(tenanycName: string, userName: string): string {
        if (!this.isMultiTenancyEnabled) {
            return userName;
        }

        return (tenanycName ? tenanycName : '.') + '\\' + userName;
    }

    getProfilePicture(): void {
        this._profileService.getProfilePicture().subscribe(result => {
            if (result && result.profilePicture) {
                this.profilePicture = 'data:image/jpeg;base64,' + result.profilePicture;
            }
        });
    }

    block(user: FriendDto): void {
        let blockUserInput = new BlockUserInput();
        blockUserInput.tenantId = user.friendTenantId;
        blockUserInput.userId = user.friendUserId;

        this._friendshipService.blockUser(blockUserInput).subscribe(() => {
            this.notify.info(this.l('UserBlocked'));
        });
    }

    unblock(user: FriendDto): void {
        let unblockUserInput = new UnblockUserInput();
        unblockUserInput.tenantId = user.friendTenantId;
        unblockUserInput.userId = user.friendUserId;

        this._friendshipService.unblockUser(unblockUserInput).subscribe(() => {
            this.notify.info(this.l('UserUnblocked'));
        });
    }

    markAllUnreadMessagesOfUserAsRead(user: ChatFriendDto): void {
        if (!user || !this.isOpen) {
            return;
        }

        var unreadMessages = _.filter(user.messages, m => m.readState === AppChatMessageReadState.Unread);
        var unreadMessageIds = _.map(unreadMessages, 'id');

        if (!unreadMessageIds.length) {
            return;
        }

        let input = new MarkAllUnreadMessagesOfUserAsReadInput();
        input.tenantId = user.friendTenantId;
        input.userId = user.friendUserId;

        this._chatService.markAllUnreadMessagesOfUserAsRead(input).subscribe(() => {
            _.forEach(user.messages, message => {
                if (unreadMessageIds.indexOf(message.id) >= 0) {
                    message.readState = AppChatMessageReadState.Read;
                }
            });
        });
    }

    loadMessages(user: ChatFriendDto, callback: any): void {
        this.loadingPreviousUserMessages = true;

        var minMessageId = undefined;
        if (user.messages && user.messages.length) {
            minMessageId = _.min(_.map(user.messages, m => m.id));
        }

        this._chatService.getUserChatMessages(user.friendTenantId ? user.friendTenantId : undefined, user.friendUserId, minMessageId)
            .subscribe(result => {
                if (!user.messages) {
                    user.messages = [];
                }

                user.messages = result.items.concat(user.messages);
                this.markAllUnreadMessagesOfUserAsRead(user);

                if (!result.items.length) {
                    user.allPreviousMessagesLoaded = true;
                }

                this.loadingPreviousUserMessages = false;
                if (callback) {
                    callback();
                }
            });
    }

    openSearchModal(userName: string, tenantId?: number): void {
        this.userLookupModal.filterText = userName;
        this.userLookupModal.show();
    }

    addFriendSelected(item: NameValueDto): void {
        var userId = item.value;
        let input = new CreateFriendshipRequestInput();
        input.userId = parseInt(userId);
        input.tenantId = this._appSessionService.tenant ? this._appSessionService.tenant.id : null;

        this._friendshipService.createFriendshipRequest(input).subscribe(() => {
            this.userNameFilter = '';
        });
    }

    search(): void {
        let input = new CreateFriendshipRequestByUserNameInput();

        if (this.userNameFilter.indexOf('\\') === -1) {
            input.userName = this.userNameFilter;
        } else {
            let tenancyAndUserNames = this.userNameFilter.split('\\');
            input.tenancyName = tenancyAndUserNames[0];
            input.userName = tenancyAndUserNames[1];
        }

        if (!input.tenancyName || !this.interTenantChatAllowed) {
            let tenantId = this._appSessionService.tenant ? this._appSessionService.tenant.id : null;
            this.openSearchModal(input.userName, tenantId);
        } else {
            this._friendshipService.createFriendshipRequestByUserName(input).subscribe(() => {
                this.userNameFilter = '';
            });
        }
    }

    getFriendOrNull(userId: number, tenantId?: number): ChatFriendDto {
        var friends = _.filter(this.friends, friend => friend.friendUserId === userId && friend.friendTenantId === tenantId);
        if (friends.length) {
            return friends[0];
        }

        return null;
    }

    getFilteredFriends(state: FriendDtoState, userNameFilter: string): FriendDto[] {
        let foundFriends = _.filter(this.friends, friend => friend.state === state &&
            this.getShownUserName(friend.friendTenancyName, friend.friendUserName)
                .toLocaleLowerCase()
                .indexOf(userNameFilter.toLocaleLowerCase()) >= 0);

        return foundFriends;
    }

    getFilteredFriendsCount(state: FriendDtoState): number {
        return _.filter(this.friends, friend => friend.state === state).length;
    }

    getUserNameByChatSide(chatSide: ChatMessageDtoSide): string {
        return chatSide === AppChatSide.Sender ?
            this.currentUser.userName :
            this.selectedUser.friendUserName;
    }

    getFixedMessageTime(messageTime: moment.Moment): string {
        return moment(messageTime).add(-1 * this.serverClientTimeDifference, 'seconds').format('YYYY-MM-DDTHH:mm:ssZ');
    }

    changeNotifyPosition(positionClass: string): void {
        if (!toastr) {
            return;
        }

        toastr.clear();
        toastr.options.positionClass = positionClass;
    };

    getFriendsAndSettings(callback: any): void {
        this._chatService.getUserChatFriendsWithSettings().subscribe(result => {
            this.friends = (result.friends as ChatFriendDto[]);
            this.serverClientTimeDifference = moment(abp.clock.now()).diff(result.serverTime, 'seconds');

            this.triggerUnreadMessageCountChangeEvent();
            callback();
        });
    }

    scrollToBottom(): void {
        setTimeout(() => {
            var $scrollArea = $('.page-quick-sidebar-chat-user-messages');
            var scrollToVal = $scrollArea.prop('scrollHeight') + 'px';
            $scrollArea.slimScroll({ scrollTo: scrollToVal });
        }, 100);
    }

    loadLastState(): void {
        let self = this;
        self._localStorageService.getItem('app.chat.isOpen', (err, isOpen) => {
            self.isOpen = isOpen;

            self._localStorageService.getItem('app.chat.pinned', (err, pinned) => {
                self.pinned = pinned;
            });

            if (isOpen) {
                $('body').addClass('page-quick-sidebar-open').promise().done(() => {
                    self._localStorageService.getItem('app.chat.selectedUser', (err, selectedUser) => {
                        if (selectedUser && selectedUser.friendUserId) {
                            $('.page-quick-sidebar-chat').addClass('page-quick-sidebar-content-item-shown');
                            self.selectFriend(selectedUser);
                        } else {
                            $('.page-quick-sidebar-chat').removeClass('page-quick-sidebar-content-item-shown');
                        }
                    });
                });
            }
        });
    }

    selectFriend(friend: ChatFriendDto): void {
        let chatUser = this.getFriendOrNull(friend.friendUserId, friend.friendTenantId);
        this.selectedUser = chatUser;
        if (!chatUser) {
            return;
        }

        this.chatMessage = '';

        if (!chatUser.messagesLoaded) {
            this.loadMessages(chatUser, () => {
                chatUser.messagesLoaded = true;
                this.scrollToBottom();
                this.$_chatMessageInput.focus();
            });
        } else {
            this.markAllUnreadMessagesOfUserAsRead(this.selectedUser);
            this.scrollToBottom();
            this.$_chatMessageInput.focus();
        }
    }

    sendMessage(): void {
        if (!this.chatMessage) {
            return;
        }

        this.sendingMessage = true;
        let tenancyName = this._appSessionService.tenant ? this._appSessionService.tenant.tenancyName : null;
        this._chatSignalrService.sendMessage({
            tenantId: this.selectedUser.friendTenantId,
            userId: this.selectedUser.friendUserId,
            message: this.chatMessage,
            tenancyName: tenancyName,
            userName: this._appSessionService.user.userName,
            profilePictureId: this._appSessionService.user.profilePictureId
        }, () => {
            this.chatMessage = '';
            this.sendingMessage = false;
        });
    }

    reversePinned(): void {
        this.pinned = !this.pinned;
    }

    bindUiEvents(): void {
        let self = this;
        self._quickSideBarChat.init((e, pos) => {
            if (pos === 0 && !this.selectedUser.allPreviousMessagesLoaded && !this.loadingPreviousUserMessages) {
                self.loadMessages(self.selectedUser, null);
            }
        });

        let $backToList = $('.page-quick-sidebar-back-to-list');
        $backToList.on('click', () => {
            self.selectedUser = new ChatFriendDto();
        });

        var $sidebarTogglers = $('.dropdown-quick-sidebar-toggler a, .page-quick-sidebar-toggler, .quick-sidebar-toggler');
        $sidebarTogglers.on('click', () => {
            this.isOpen = $('body').hasClass('page-quick-sidebar-open');
        });

        //Close chat panel when mouse moved outside of it, if not pinned
        $('div.page-quick-sidebar-wrapper').on('mouseleave', () => {
            if (this.pinned) {
                return;
            }

            $('body').removeClass('page-quick-sidebar-open');
            this.isOpen = false;
        });
    }

    ngAfterViewInit(): void {
        this.$_chatMessageInput = $("#ChatMessage");
    }

    adjustNotifyPosition(): void {
        if (this.isOpen) {
            this.changeNotifyPosition('toast-chat-open');
        } else {
            this.changeNotifyPosition('toast-bottom-right');
        }
    }

    triggerUnreadMessageCountChangeEvent(): void {
        let totalUnreadMessageCount = 0;

        if (this.friends) {
            totalUnreadMessageCount = _.reduce(this.friends, (memo, friend) => memo + friend.unreadMessageCount, 0);
        }

        abp.event.trigger('app.chat.unreadMessageCountChanged', totalUnreadMessageCount);
    }

    registerEvents(): void {
        let self = this;

        abp.event.on('app.chat.messageReceived', message => {
            var user = this.getFriendOrNull(message.targetUserId, message.targetTenantId);
            if (!user) {
                return;
            }

            user.messages = user.messages || [];
            user.messages.push(message);

            if (message.side === AppChatSide.Receiver) {
                user.unreadMessageCount += 1;
                message.readState = AppChatMessageReadState.Unread;
                this.triggerUnreadMessageCountChangeEvent();

                if (this.isOpen && this.selectedUser !== null && user.friendTenantId === this.selectedUser.friendTenantId && user.friendUserId === this.selectedUser.friendUserId) {
                    this.markAllUnreadMessagesOfUserAsRead(user);
                } else {
                    this.notify.info(
                        abp.utils.formatString('{0}: {1}', user.friendUserName, abp.utils.truncateString(message.message, 100)),
                        null,
                        {
                            onclick() {
                                if (!$('body').hasClass('page-quick-sidebar-open')) {
                                    $('body').addClass('page-quick-sidebar-open');
                                    self.isOpen = true;
                                }

                                if (!$('.page-quick-sidebar-chat').hasClass('page-quick-sidebar-content-item-shown')) {
                                    $('.page-quick-sidebar-chat').addClass('page-quick-sidebar-content-item-shown');
                                }

                                self.selectFriend(user);
                                self.pinned = true;
                            }
                        });
                }
            }

            self.scrollToBottom();
        });

        abp.event.on('app.chat.friendshipRequestReceived', (data, isOwnRequest) => {
            if (!isOwnRequest) {
                abp.notify.info(abp.utils.formatString(this.l('UserSendYouAFriendshipRequest'), data.friendUserName));
            }

            if (!_.filter(this.friends, f => f.friendUserId === data.friendUserId && f.friendTenantId === data.friendTenantId).length) {
                this.friends.push(data);
            }
        });

        abp.event.on('app.chat.userConnectionStateChanged', data => {
            var user = this.getFriendOrNull(data.friend.userId, data.friend.tenantId);
            if (!user) {
                return;
            }

            user.isOnline = data.isConnected;
        });

        abp.event.on('app.chat.userStateChanged', data => {
            var user = this.getFriendOrNull(data.friend.userId, data.friend.tenantId);
            if (!user) {
                return;
            }

            user.state = data.state;
        });

        abp.event.on('app.chat.allUnreadMessagesOfUserRead', data => {
            var user = this.getFriendOrNull(data.friend.userId, data.friend.tenantId);
            if (!user) {
                return;
            }

            user.unreadMessageCount = 0;
            this.triggerUnreadMessageCountChangeEvent();
        });

        abp.event.on('app.chat.connected', () => {
            let self = this;
            self.getFriendsAndSettings(() => {
                DomHelper.waitUntilElementIsReady('.page-quick-sidebar-wrapper, .quick-sidebar-toggler', () => {
                    self.bindUiEvents();

                    DomHelper.waitUntilElementIsReady('.page-quick-sidebar-chat', () => {
                        self.loadLastState();
                    });
                });
            });
        });
    }
    
    init(): void {
        this.registerEvents();
        this.userLookupModal.configure({
            title: this.l('SelectAUser'),
            dataSource: (skipCount: number, maxResultCount: number, filter: string, tenantId?: number) => {
                var input = new FindUsersInput();
                input.filter = filter;
                input.maxResultCount = maxResultCount;
                input.skipCount = skipCount;
                input.tenantId = tenantId;
                return this._commonLookupService.findUsers(input);
            }
        });

        this.getProfilePicture();

        this.tenantToTenantChatAllowed = this.feature.isEnabled('App.ChatFeature.TenantToTenant');
        this.tenantToHostChatAllowed = this.feature.isEnabled('App.ChatFeature.TenantToHost');
        this.interTenantChatAllowed = this.feature.isEnabled('App.ChatFeature.TenantToTenant') || this.feature.isEnabled('App.ChatFeature.TenantToHost') || !this._appSessionService.tenant;
    }
}
