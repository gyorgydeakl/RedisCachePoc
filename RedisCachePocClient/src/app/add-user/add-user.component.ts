import {Component, inject, output} from '@angular/core';
import {CreateUserDto, MovieReviewerClient, UserDto} from '../../client';
import {ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';
import {FormsModule} from '@angular/forms';
import {Textarea} from 'primeng/textarea';
import {InputText} from 'primeng/inputtext';
import {MessageService} from 'primeng/api';

@Component({
  selector: 'app-add-user',
  imports: [
    ButtonLabel,
    ButtonIcon,
    ButtonDirective,
    FormsModule,
    Textarea,
    InputText
  ],
  templateUrl: './add-user.component.html',
  styleUrl: './add-user.component.css'
})
export class AddUserComponent {
  client = inject(MovieReviewerClient);
  messageService = inject(MessageService);
  userCreated = output<UserDto>();
  command: CreateUserDto = {
    username: '',
    bio: '',
    email: ''
  }
  createUser() {
    this.client.usersPost(this.command).subscribe(u => {
      this.userCreated.emit(u);
      this.command = { username: '', bio: '', email: '' };
      this.messageService.add({
        severity: 'success',
        summary: 'User created',
        detail: `User '${u.username}' created!`
      });
    });
  }
}
