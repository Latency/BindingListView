/****************************************************************************
 * File:      InvalidSourceListException.cs
 * Solution:  PSL-Simulator
 * Date:      03/05/2015
 * Author:    Latency McLaughlin
 ****************************************************************************/

using System;
using System.Runtime.Serialization;
using Equin.ApplicationFramework.Properties;

namespace Equin.ApplicationFramework {
  [Serializable]
  public class InvalidSourceListException : Exception {
    public InvalidSourceListException()
      : base(Resources.InvalidSourceList) { }

    public InvalidSourceListException(string message)
      : base(message) { }

    public InvalidSourceListException(SerializationInfo info, StreamingContext context)
      : base(info, context) { }
  }
}