import { Component, OnInit } from '@angular/core';
import { take } from 'rxjs/operators';
import { Message } from '../_models/message';
import { Pagination } from '../_models/pagination';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages: Message[];
  pagination: Pagination;
  container = 'Unread';
  pageNumber = 1;
  pageSize = 6;
  loading = false;
  user: User;

  constructor(private messageService: MessageService, private accountService: AccountService) { 
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => this.user = user);
  }

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    this.loading = true;
    this.messageService.getMessages(this.pageNumber, this.pageSize, this.container).subscribe(messages => {
      this.messages = messages.result;
      this.pagination = messages.pagination;
      this.loading = false;
    })
  }


  deleteMessage(photoId: number) {
    this.messageService.deleteMessage(photoId).subscribe(() => {
      this.messages.splice(this.messages.findIndex(x => x.id === photoId), 1);
    });
  }

  unsendMessage(photoId: number) {
    this.messageService.unsendMessage(photoId).subscribe(() => {
      this.messages.splice(this.messages.findIndex(x => x.id === photoId), 1);
    })
  }

  pageChanged(event: any) {
    if (this.pageNumber != event.page){
      this.pageNumber = event.page;
      this.loadMessages();
    }
  }

}
