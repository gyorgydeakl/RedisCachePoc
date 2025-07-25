import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MovieReviewerClient, MovieSummaryDto } from '../../client';

/* PrimeNG */
import { Dialog } from 'primeng/dialog';
import { DataView } from 'primeng/dataview';
import { Card } from 'primeng/card';
import { Tag } from 'primeng/tag';
import {Button, ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';
import { PrimeTemplate } from 'primeng/api';

/* Your form component */
import { AddMovieComponent } from '../add-movie/add-movie.component';

@Component({
  selector: 'app-movie-list',
  standalone: true,
  imports: [
    Dialog,
    DataView,
    Card,
    Tag,
    PrimeTemplate,
    AddMovieComponent,
    ButtonDirective,
    ButtonLabel,
    ButtonIcon,
  ],
  templateUrl: './movie-list.component.html',
  styleUrl: './movie-list.component.css',
})
export class MovieListComponent implements OnInit {
  private readonly client = inject(MovieReviewerClient);
  private readonly router = inject(Router);

  protected readonly movies = signal<MovieSummaryDto[]>([]);
  protected readonly showAdd = signal(false);

  ngOnInit() {
    this.reload();
  }
  reload() {
    this.client.moviesGet().subscribe(dtos => this.movies.set(dtos));
  }
  goTo(id: string) {
    this.router.navigate(['/movies', id]);
  }
  openAdd() {
    this.showAdd.set(true);
  }
  closedAdd() {
    this.reload();
  }
  onMovieCreated() {
    this.showAdd.set(false);
    this.reload();
  }
}
