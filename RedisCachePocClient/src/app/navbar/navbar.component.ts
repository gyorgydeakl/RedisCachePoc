import {Component, inject} from '@angular/core';
import {ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';
import {RouterLink} from '@angular/router';
import {MovieReviewerClient} from '../../client';
import {MessageService} from 'primeng/api';

@Component({
  selector: 'app-navbar',
  imports: [
    ButtonIcon,
    ButtonLabel,
    ButtonDirective,
    RouterLink
  ],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent {
  client = inject(MovieReviewerClient)
  messageService = inject(MessageService)
  clearCache() {
    this.client.cacheClearDelete().subscribe(
      _ => this.messageService.add({
        severity: 'success',
        summary: 'Cache cleared',
        detail: 'Cache cleared!'
      })
    )
  }
}
