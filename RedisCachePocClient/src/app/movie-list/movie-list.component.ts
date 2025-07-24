import {ChangeDetectionStrategy, Component, inject, OnInit, signal} from '@angular/core';
import {MovieReviewerClient, MovieSummaryDto} from '../../client';
import {Button} from 'primeng/button';

@Component({
  selector: 'app-movie-list',
  imports: [
    Button
  ],
  templateUrl: './movie-list.component.html',
  styleUrl: './movie-list.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MovieListComponent implements OnInit {
  private readonly client = inject(MovieReviewerClient)
  protected readonly movies = signal<MovieSummaryDto[]>([]);
  ngOnInit() {
    this.client.moviesGet().subscribe(dtos => this.movies.set(dtos));
  }
}
