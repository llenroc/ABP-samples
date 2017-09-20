import { FriendDto, ChatMessageDto } from '@shared/service-proxies/service-proxies';

export class ChatFriendDto extends FriendDto {
    messages: ChatMessageDto[];
    allPreviousMessagesLoaded: boolean = false;
    messagesLoaded: boolean = false;
}