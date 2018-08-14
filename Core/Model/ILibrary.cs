﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jammit.Model
{
  public interface ILibrary
  {
    List<SongInfo> Songs { get; }

    void AddSong(SongInfo song);

    void RemoveSong(Guid id);
  }
}
