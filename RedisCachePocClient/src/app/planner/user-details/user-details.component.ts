import {Component, inject, input, resource} from '@angular/core';
import {MoviePlannerClient, UserDto} from '../../../planner-client';
import {firstValueFrom} from 'rxjs';
import {ProgressSpinner} from 'primeng/progressspinner';
import {Message} from 'primeng/message';
import {Card} from 'primeng/card';
import {Avatar} from 'primeng/avatar';
import {PrimeTemplate} from 'primeng/api';
import {UpperCasePipe} from '@angular/common';
import {WatchlistComponent} from '../watchlist/watchlist.component';
import {resourceObs} from '../../utils';

@Component({
  selector: 'app-user-details',
  imports: [
    ProgressSpinner,
    Message,
    Card,
    Avatar,
    PrimeTemplate,
    UpperCasePipe,
    WatchlistComponent
  ],
  templateUrl: './user-details.component.html',
  styleUrl: './user-details.component.css'
})
export class UserDetailsComponent {
  readonly client = inject(MoviePlannerClient);
  readonly userId = input.required<string>();
  readonly user = resourceObs(() => this.userId(), param => this.client.getUser(param))
}
