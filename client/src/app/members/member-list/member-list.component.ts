import { Component, OnInit } from '@angular/core';
import { Observable, pipe } from 'rxjs';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { User } from 'src/app/_models/user';
import { UserParams } from 'src/app/_models/userParams';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  members: Member[];
  pagination: Pagination;
  userParams: UserParams;
  user: User;
  genderList = [{value: 'male', display: 'Males'}, {value: 'female', display: 'Females'}]

  constructor(private membersService: MembersService) {
    this.userParams = this.membersService.getUserParams();
   }

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers() {
    this.membersService.setUserParams(this.userParams);
    this.membersService.getMembers(this.userParams).subscribe(res => {
      this.members = res.result;
      this.pagination = res.pagination;
    })
  }

  resetFilters() {
    this. userParams = this.membersService.resetUserParams();
    this.loadMembers();
  }

  pageChanged(event: any) {
    if(this.userParams.pageNumber != event.page){
      this.userParams.pageNumber = event.page;
      this.membersService.setUserParams(this.userParams);
      this.loadMembers();
    }
    
  }
  
}
