﻿/*
 * DRBDBReader
 * Copyright (C) 2017, Kyle Repinski
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Text;

namespace DRBDBReader.DB.Records
{
	public class StringRecord : Record
	{
		private const byte FIELD_ID = 0;
		private const byte FIELD_LOCATION = 1;
		private const byte FIELD_OBD_CODE_STR = 3;

		public ushort id;

		private uint location;
		public byte textTableNumber;
		public uint textTableOffset;

		public string text;

		private byte[] obdCodeBytes;
		public string obdCodeString;

		public StringRecord( Table table, byte[] record ) : base( table, record )
		{
			this.id = (ushort)this.table.readField( this, FIELD_ID );

			this.location = (uint)this.table.readField( this, FIELD_LOCATION );
			this.textTableNumber = (byte)( this.location >> 24 );
			this.textTableOffset = this.location & 0xFFFFFF;

			this.text = this.readText();

			this.obdCodeBytes = this.table.readFieldRaw( this, FIELD_OBD_CODE_STR );

			int firstNullChar = Array.IndexOf( this.obdCodeBytes, (byte)0 );
			if( firstNullChar == -1 )
			{
				firstNullChar = this.obdCodeBytes.Length;
			}
			this.obdCodeString = Encoding.ASCII.GetString( this.obdCodeBytes, 0, firstNullChar );
		}

		private string readText()
		{
			Table txtTable = this.table.db.tables[Database.TABLE_DBTEXT_1 + this.textTableNumber];

			uint row = (uint)Math.Floor( (double)( this.textTableOffset / txtTable.rowSize ) );
			uint rowOffset = this.textTableOffset % txtTable.rowSize;
			Record txtRecord = txtTable.records[row];
			byte curByte;

			StringBuilder sb = new StringBuilder();

			while( ( curByte = txtRecord.record[rowOffset++] ) != 0 )
			{
				sb.Append( Convert.ToChar( curByte ) );
				if( rowOffset >= txtTable.rowSize )
				{
					rowOffset = 0;
					txtRecord = txtTable.records[++row];
				}
			}

			return sb.ToString();
		}
	}
}
