import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MoviePlannerClient } from '../../../planner-client';
import { UserDto } from '../../../reviewer-client';

import {Button, ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';
import {TableModule} from 'primeng/table';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [ButtonDirective, ButtonLabel, TableModule, ButtonIcon],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.css'
})
export class UserListComponent implements OnInit {
  private readonly client  = inject(MoviePlannerClient);
  private readonly router  = inject(Router);

  readonly users = signal<UserDto[]>([]);

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.client.getUsers().subscribe(u => this.users.set(u));
  }

  view(id: string) {
    this.router.navigate(['planner/watchlist', id]);
  }
}
